using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Examinations.Commands.CreateExamination;
using ZealEducation.Application.UnitTests.Handlers.Batches;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Examinations;

public class CreateExaminationCommandHandlerTests
{
    private readonly Mock<IRepository<Batch>> _batchRepo = new();
    private readonly Mock<IRepository<Examination>> _examRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private CreateExaminationCommandHandler CreateHandler() =>
        new(_batchRepo.Object, _examRepo.Object, _staffRepo.Object, _currentUser.Object, _uow.Object);

    private static CreateExaminationCommand Cmd(
        Guid? batchId = null,
        string examName = "Midterm",
        DateOnly? examDate = null,
        string? location = "Hall 1",
        int maxScore = 100,
        int passScore = 50) => new(
            batchId ?? Guid.NewGuid(),
            examName,
            examDate ?? new DateOnly(2030, 3, 15),
            location,
            maxScore,
            passScore);

    private static Batch ExistingBatch(
        Guid batchId,
        BatchStatus status = BatchStatus.Active,
        DateOnly? start = null,
        DateOnly? end = null) => new()
    {
        Id = batchId,
        BatchCode = "B-001",
        CourseId = Guid.NewGuid(),
        StartDate = start ?? new DateOnly(2030, 1, 1),
        EndDate = end ?? new DateOnly(2030, 6, 1),
        Status = status
    };

    private static Staff ExistingStaff(Guid userAccountId) => new()
    {
        Id = Guid.NewGuid(),
        UserAccountId = userAccountId,
        Position = "Lecturer",
        Department = "CS",
        JoinedDate = new DateOnly(2020, 1, 1),
        IsActive = true
    };

    private void SetupStaffQuery(IEnumerable<Staff> data)
    {
        var queryable = new TestAsyncEnumerable<Staff>(data);
        _staffRepo.Setup(r => r.Query()).Returns(queryable);
    }

    private void SetupExamQuery(IEnumerable<Examination> data)
    {
        var queryable = new TestAsyncEnumerable<Examination>(data);
        _examRepo.Setup(r => r.Query()).Returns(queryable);
    }

    [Fact]
    public async Task Throws_UnauthorizedException_when_current_user_is_null()
    {
        _currentUser.Setup(c => c.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _examRepo.Verify(r => r.AddAsync(It.IsAny<Examination>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_batch_does_not_exist()
    {
        var batchId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _batchRepo.SetupGetById(batchId, null);

        var act = async () => await CreateHandler().Handle(Cmd(batchId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _examRepo.Verify(r => r.AddAsync(It.IsAny<Examination>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_batch_is_completed()
    {
        var batchId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, status: BatchStatus.Completed));

        var act = async () => await CreateHandler().Handle(Cmd(batchId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot add examinations to a completed or cancelled batch.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_batch_is_cancelled()
    {
        var batchId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId, status: BatchStatus.Cancelled));

        var act = async () => await CreateHandler().Handle(Cmd(batchId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot add examinations to a completed or cancelled batch.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_exam_date_is_before_batch_start()
    {
        var batchId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _batchRepo.SetupGetById(batchId,
            ExistingBatch(batchId, start: new DateOnly(2030, 1, 1), end: new DateOnly(2030, 6, 1)));

        var act = async () => await CreateHandler().Handle(
            Cmd(batchId, examDate: new DateOnly(2029, 12, 31)),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .Where(e => e.Message.Contains("Exam date must fall within the batch period"));
    }

    [Fact]
    public async Task Throws_ConflictException_when_exam_date_is_after_batch_end()
    {
        var batchId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(Guid.NewGuid());
        _batchRepo.SetupGetById(batchId,
            ExistingBatch(batchId, start: new DateOnly(2030, 1, 1), end: new DateOnly(2030, 6, 1)));

        var act = async () => await CreateHandler().Handle(
            Cmd(batchId, examDate: new DateOnly(2030, 7, 1)),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .Where(e => e.Message.Contains("Exam date must fall within the batch period"));
    }

    [Fact]
    public async Task Throws_ConflictException_when_current_user_is_not_staff()
    {
        var batchId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupStaffQuery(Array.Empty<Staff>());

        var act = async () => await CreateHandler().Handle(Cmd(batchId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Current user is not registered as staff.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_duplicate_examination_already_exists()
    {
        var batchId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var examDate = new DateOnly(2030, 3, 15);
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupStaffQuery(new[] { ExistingStaff(userId) });
        SetupExamQuery(new[]
        {
            new Examination
            {
                Id = Guid.NewGuid(),
                BatchId = batchId,
                ExamName = "Midterm",
                ExamDate = examDate
            }
        });

        var act = async () => await CreateHandler().Handle(
            Cmd(batchId, examName: "Midterm", examDate: examDate),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("An examination with this name already exists on the same date for this batch.");
        _examRepo.Verify(r => r.AddAsync(It.IsAny<Examination>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Creates_examination_when_command_is_valid()
    {
        var batchId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var staff = ExistingStaff(userId);
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupStaffQuery(new[] { staff });
        SetupExamQuery(Array.Empty<Examination>());

        Examination? captured = null;
        _examRepo.Setup(r => r.AddAsync(It.IsAny<Examination>(), It.IsAny<CancellationToken>()))
            .Callback<Examination, CancellationToken>((e, _) => captured = e)
            .ReturnsAsync((Examination e, CancellationToken _) => e);

        var examDate = new DateOnly(2030, 3, 15);
        var resultId = await CreateHandler().Handle(
            Cmd(batchId, examName: "Midterm", examDate: examDate, location: "Hall 1", maxScore: 100, passScore: 50),
            default);

        captured.Should().NotBeNull();
        captured!.Id.Should().NotBe(Guid.Empty);
        resultId.Should().Be(captured.Id);
        captured.BatchId.Should().Be(batchId);
        captured.ExamName.Should().Be("Midterm");
        captured.ExamDate.Should().Be(examDate);
        captured.Location.Should().Be("Hall 1");
        captured.MaxScore.Should().Be(100);
        captured.PassScore.Should().Be(50);
        captured.ScheduledById.Should().Be(staff.Id);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Trims_exam_name_and_location_before_saving()
    {
        var batchId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupStaffQuery(new[] { ExistingStaff(userId) });
        SetupExamQuery(Array.Empty<Examination>());

        Examination? captured = null;
        _examRepo.Setup(r => r.AddAsync(It.IsAny<Examination>(), It.IsAny<CancellationToken>()))
            .Callback<Examination, CancellationToken>((e, _) => captured = e)
            .ReturnsAsync((Examination e, CancellationToken _) => e);

        await CreateHandler().Handle(
            Cmd(batchId, examName: "  Final Exam  ", location: "  Auditorium  "),
            default);

        captured!.ExamName.Should().Be("Final Exam");
        captured.Location.Should().Be("Auditorium");
    }

    [Fact]
    public async Task Stores_null_location_when_input_is_whitespace()
    {
        var batchId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupStaffQuery(new[] { ExistingStaff(userId) });
        SetupExamQuery(Array.Empty<Examination>());

        Examination? captured = null;
        _examRepo.Setup(r => r.AddAsync(It.IsAny<Examination>(), It.IsAny<CancellationToken>()))
            .Callback<Examination, CancellationToken>((e, _) => captured = e)
            .ReturnsAsync((Examination e, CancellationToken _) => e);

        await CreateHandler().Handle(Cmd(batchId, location: "   "), default);

        captured!.Location.Should().BeNull();
    }

    [Fact]
    public async Task Stores_null_location_when_input_is_null()
    {
        var batchId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _batchRepo.SetupGetById(batchId, ExistingBatch(batchId));
        SetupStaffQuery(new[] { ExistingStaff(userId) });
        SetupExamQuery(Array.Empty<Examination>());

        Examination? captured = null;
        _examRepo.Setup(r => r.AddAsync(It.IsAny<Examination>(), It.IsAny<CancellationToken>()))
            .Callback<Examination, CancellationToken>((e, _) => captured = e)
            .ReturnsAsync((Examination e, CancellationToken _) => e);

        await CreateHandler().Handle(Cmd(batchId, location: null), default);

        captured!.Location.Should().BeNull();
    }
}
