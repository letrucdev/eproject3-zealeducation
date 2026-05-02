using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Batches.Commands.AssignFaculty;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Batches;

public class AssignFacultyCommandHandlerTests
{
    private readonly Mock<IRepository<Batch>> _batchRepo = new();
    private readonly Mock<IRepository<Faculty>> _facultyRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private AssignFacultyCommandHandler CreateHandler() =>
        new(_batchRepo.Object, _facultyRepo.Object, _uow.Object);

    private static Batch ExistingBatch(
        Guid batchId,
        Guid? facultyId = null,
        BatchStatus status = BatchStatus.NeedsInstructor) => new()
    {
        Id = batchId,
        BatchCode = "B-001",
        CourseId = Guid.NewGuid(),
        FacultyId = facultyId,
        StartDate = new DateOnly(2030, 1, 1),
        EndDate = new DateOnly(2030, 6, 1),
        MaxCapacity = 30,
        Status = status
    };

    [Fact]
    public async Task Throws_NotFoundException_when_batch_does_not_exist()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, null);

        var act = async () => await CreateHandler().Handle(
            new AssignFacultyCommand(batchId, Guid.NewGuid()),
            default);

        await act.Should().ThrowAsync<NotFoundException>();
        _batchRepo.Verify(r => r.Update(It.IsAny<Batch>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_batch_is_completed()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, status: BatchStatus.Completed));

        var act = async () => await CreateHandler().Handle(
            new AssignFacultyCommand(batchId, Guid.NewGuid()),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot assign faculty to a completed or cancelled batch.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_batch_is_cancelled()
    {
        var batchId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, status: BatchStatus.Cancelled));

        var act = async () => await CreateHandler().Handle(
            new AssignFacultyCommand(batchId, null),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot assign faculty to a completed or cancelled batch.");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_faculty_does_not_exist()
    {
        var batchId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        _facultyRepo.SetupExists(facultyId, false);

        var act = async () => await CreateHandler().Handle(
            new AssignFacultyCommand(batchId, facultyId),
            default);

        await act.Should().ThrowAsync<NotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_faculty_has_overlapping_batch()
    {
        var batchId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        _facultyRepo.SetupExists(facultyId, true);
        _batchRepo.SetupFind(new[] { new Batch { Id = Guid.NewGuid(), BatchCode = "OTHER" } });

        var act = async () => await CreateHandler().Handle(
            new AssignFacultyCommand(batchId, facultyId),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Faculty already has another batch scheduled within this date range.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Activates_batch_when_assigning_faculty_to_needs_instructor_batch()
    {
        var batchId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        var batch = ExistingBatch(batchId, status: BatchStatus.NeedsInstructor);
        _batchRepo.SetupGetById(batchId, batch);
        _facultyRepo.SetupExists(facultyId, true);
        _batchRepo.SetupFind(Array.Empty<Batch>());

        await CreateHandler().Handle(new AssignFacultyCommand(batchId, facultyId), default);

        batch.FacultyId.Should().Be(facultyId);
        batch.Status.Should().Be(BatchStatus.Active);
        _batchRepo.Verify(r => r.Update(batch), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Keeps_status_when_assigning_faculty_to_already_active_batch()
    {
        var batchId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        var batch = ExistingBatch(batchId, facultyId: Guid.NewGuid(), status: BatchStatus.Active);
        _batchRepo.SetupGetById(batchId, batch);
        _facultyRepo.SetupExists(facultyId, true);
        _batchRepo.SetupFind(Array.Empty<Batch>());

        await CreateHandler().Handle(new AssignFacultyCommand(batchId, facultyId), default);

        batch.FacultyId.Should().Be(facultyId);
        batch.Status.Should().Be(BatchStatus.Active);
        _batchRepo.Verify(r => r.Update(batch), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Sets_status_to_needs_instructor_when_unassigning_faculty_from_active_batch()
    {
        var batchId = Guid.NewGuid();
        var batch = ExistingBatch(batchId, facultyId: Guid.NewGuid(), status: BatchStatus.Active);
        _batchRepo.SetupGetById(batchId, batch);

        await CreateHandler().Handle(new AssignFacultyCommand(batchId, null), default);

        batch.FacultyId.Should().BeNull();
        batch.Status.Should().Be(BatchStatus.NeedsInstructor);
        _batchRepo.Verify(r => r.Update(batch), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Keeps_status_when_unassigning_from_already_needs_instructor_batch()
    {
        var batchId = Guid.NewGuid();
        var batch = ExistingBatch(batchId, facultyId: null, status: BatchStatus.NeedsInstructor);
        _batchRepo.SetupGetById(batchId, batch);

        await CreateHandler().Handle(new AssignFacultyCommand(batchId, null), default);

        batch.FacultyId.Should().BeNull();
        batch.Status.Should().Be(BatchStatus.NeedsInstructor);
        _batchRepo.Verify(r => r.Update(batch), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
