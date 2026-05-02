using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Batches.Commands.UpdateBatch;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Batches;

public class UpdateBatchCommandHandlerTests
{
    private readonly Mock<IRepository<Batch>> _batchRepo = new();
    private readonly Mock<IRepository<Course>> _courseRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private UpdateBatchCommandHandler CreateHandler() =>
        new(_batchRepo.Object, _courseRepo.Object, _uow.Object);

    private static DateOnly Future(int days) => DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(days));

    private static UpdateBatchCommand Cmd(
        Guid batchId,
        Guid courseId,
        string batchCode = "B-001",
        DateOnly? start = null,
        DateOnly? end = null,
        string? location = "Hall 1",
        int maxCapacity = 30,
        BatchStatus status = BatchStatus.Active) => new(
            batchId,
            batchCode,
            courseId,
            start ?? Future(10),
            end ?? Future(120),
            location,
            maxCapacity,
            status);

    private static Batch ExistingBatch(
        Guid batchId,
        Guid courseId,
        Guid? facultyId = null,
        string code = "B-001",
        DateOnly? start = null,
        DateOnly? end = null,
        BatchStatus status = BatchStatus.Active) => new()
    {
        Id = batchId,
        CourseId = courseId,
        FacultyId = facultyId,
        BatchCode = code,
        StartDate = start ?? Future(10),
        EndDate = end ?? Future(120),
        Location = "Hall 1",
        MaxCapacity = 30,
        Status = status
    };

    [Fact]
    public async Task Throws_NotFoundException_when_batch_does_not_exist()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, null);

        var act = async () => await CreateHandler().Handle(Cmd(batchId, courseId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _batchRepo.Verify(r => r.Update(It.IsAny<Batch>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_changed_start_date_is_in_the_past()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var batch = ExistingBatch(batchId, courseId, start: Future(10), end: Future(120));
        _batchRepo.SetupGetById(batchId, batch);

        var past = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1));
        var act = async () => await CreateHandler().Handle(
            Cmd(batchId, courseId, start: past, end: Future(120)),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Start date must be today or later.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_batch_code_changes_to_an_existing_one()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var batch = ExistingBatch(batchId, courseId, code: "OLD-CODE");
        _batchRepo.SetupGetById(batchId, batch);
        _batchRepo.SetupFind(new[] { new Batch { Id = Guid.NewGuid(), BatchCode = "B-NEW" } });

        var act = async () => await CreateHandler().Handle(
            Cmd(batchId, courseId, batchCode: "B-NEW"),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Batch code is already in use.");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_course_changes_to_a_non_existing_course()
    {
        var batchId = Guid.NewGuid();
        var oldCourseId = Guid.NewGuid();
        var newCourseId = Guid.NewGuid();
        var batch = ExistingBatch(batchId, oldCourseId);
        _batchRepo.SetupGetById(batchId, batch);
        _courseRepo.SetupExists(newCourseId, false);

        var act = async () => await CreateHandler().Handle(
            Cmd(batchId, newCourseId),
            default);

        await act.Should().ThrowAsync<NotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_reopening_completed_batch()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var batch = ExistingBatch(batchId, courseId, status: BatchStatus.Completed);
        _batchRepo.SetupGetById(batchId, batch);

        var act = async () => await CreateHandler().Handle(
            Cmd(batchId, courseId, status: BatchStatus.Active),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot reopen a completed or cancelled batch.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_reopening_cancelled_batch()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var batch = ExistingBatch(batchId, courseId, status: BatchStatus.Cancelled);
        _batchRepo.SetupGetById(batchId, batch);

        var act = async () => await CreateHandler().Handle(
            Cmd(batchId, courseId, status: BatchStatus.NeedsInstructor),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot reopen a completed or cancelled batch.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_activating_without_faculty()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var batch = ExistingBatch(batchId, courseId, facultyId: null, status: BatchStatus.NeedsInstructor);
        _batchRepo.SetupGetById(batchId, batch);

        var act = async () => await CreateHandler().Handle(
            Cmd(batchId, courseId, status: BatchStatus.Active),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot activate a batch without an assigned faculty.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_faculty_has_overlapping_batch_after_date_change()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        var batch = ExistingBatch(batchId, courseId, facultyId: facultyId,
            start: Future(10), end: Future(120), status: BatchStatus.Active);
        _batchRepo.SetupGetById(batchId, batch);
        _batchRepo.SetupFind(new[]
        {
            new Batch { Id = Guid.NewGuid(), FacultyId = facultyId, BatchCode = "OTHER" }
        });

        var act = async () => await CreateHandler().Handle(
            Cmd(batchId, courseId, batchCode: "B-001",
                start: Future(15), end: Future(125),
                status: BatchStatus.Active),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Faculty already has another batch scheduled within this date range.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Updates_batch_when_command_is_valid()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var batch = ExistingBatch(batchId, courseId, code: "OLD", status: BatchStatus.Active);
        _batchRepo.SetupGetById(batchId, batch);
        _batchRepo.SetupFind(Array.Empty<Batch>());

        var newStart = Future(20);
        var newEnd = Future(200);
        await CreateHandler().Handle(
            Cmd(batchId, courseId,
                batchCode: "B-NEW",
                start: newStart,
                end: newEnd,
                location: "Room 9",
                maxCapacity: 50,
                status: BatchStatus.Active),
            default);

        batch.BatchCode.Should().Be("B-NEW");
        batch.CourseId.Should().Be(courseId);
        batch.StartDate.Should().Be(newStart);
        batch.EndDate.Should().Be(newEnd);
        batch.Location.Should().Be("Room 9");
        batch.MaxCapacity.Should().Be(50);
        batch.Status.Should().Be(BatchStatus.Active);

        _batchRepo.Verify(r => r.Update(batch), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Trims_batch_code_and_normalizes_location()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var batch = ExistingBatch(batchId, courseId, code: "OLD", status: BatchStatus.Active);
        _batchRepo.SetupGetById(batchId, batch);
        _batchRepo.SetupFind(Array.Empty<Batch>());

        await CreateHandler().Handle(
            Cmd(batchId, courseId,
                batchCode: "  B-TRIM  ",
                location: "  Hall 5  ",
                status: BatchStatus.Active),
            default);

        batch.BatchCode.Should().Be("B-TRIM");
        batch.Location.Should().Be("Hall 5");
    }

    [Fact]
    public async Task Sets_location_to_null_when_blank()
    {
        var batchId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var batch = ExistingBatch(batchId, courseId, status: BatchStatus.Active);
        _batchRepo.SetupGetById(batchId, batch);
        _batchRepo.SetupFind(Array.Empty<Batch>());

        await CreateHandler().Handle(
            Cmd(batchId, courseId, location: "   ", status: BatchStatus.Active),
            default);

        batch.Location.Should().BeNull();
    }
}
