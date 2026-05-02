using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.ExamResults.Commands.OverrideExamResult;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.ExamResults;

public class OverrideExamResultCommandHandlerTests
{
    private readonly Mock<IRepository<ExamResult>> _examResultRepo = new();
    private readonly Mock<IRepository<Examination>> _examinationRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private OverrideExamResultCommandHandler CreateHandler() => new(
        _examResultRepo.Object,
        _examinationRepo.Object,
        _staffRepo.Object,
        _currentUser.Object,
        _uow.Object);

    private static OverrideExamResultCommand Cmd(
        Guid resultId,
        decimal score = 90m,
        string overrideReason = "Re-evaluated answer sheet") =>
        new(resultId, score, overrideReason);

    private static ExamResult ExistingResult(Guid resultId, Guid examId) => new()
    {
        Id = resultId,
        ExamId = examId,
        EnrollmentId = Guid.NewGuid(),
        Score = 50m,
        Grade = "D",
        IsPassed = true,
        IsFinalized = false,
        IsOverridden = false,
        GradedById = Guid.NewGuid(),
        GradedAt = DateTime.UtcNow.AddDays(-1)
    };

    private static Examination ExistingExam(Guid examId, int maxScore = 100, int passScore = 50) => new()
    {
        Id = examId,
        BatchId = Guid.NewGuid(),
        ExamName = "Final",
        ExamDate = new DateOnly(2030, 5, 1),
        MaxScore = maxScore,
        PassScore = passScore,
        ScheduledById = Guid.NewGuid()
    };

    private void SetupStaffQuery(IEnumerable<Staff> data)
    {
        var queryable = new TestAsyncEnumerable<Staff>(data);
        _staffRepo.Setup(r => r.Query()).Returns(queryable);
    }

    [Fact]
    public async Task Throws_UnauthorizedException_when_current_user_is_not_resolved()
    {
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(Cmd(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _examResultRepo.Verify(r => r.Update(It.IsAny<ExamResult>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_result_does_not_exist()
    {
        var resultId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _examResultRepo.SetupGetById(resultId, null);

        var act = async () => await CreateHandler().Handle(Cmd(resultId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_NotFoundException_when_examination_does_not_exist()
    {
        var resultId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _examResultRepo.SetupGetById(resultId, ExistingResult(resultId, examId));
        _examinationRepo.SetupGetById(examId, null);

        var act = async () => await CreateHandler().Handle(Cmd(resultId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_ConflictException_when_score_exceeds_max_score()
    {
        var resultId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _examResultRepo.SetupGetById(resultId, ExistingResult(resultId, examId));
        _examinationRepo.SetupGetById(examId, ExistingExam(examId, maxScore: 100));

        var act = async () => await CreateHandler().Handle(Cmd(resultId, score: 200m), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Score must not exceed the exam's max score (100).");
    }

    [Fact]
    public async Task Throws_ConflictException_when_current_user_is_not_a_staff()
    {
        var resultId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _examResultRepo.SetupGetById(resultId, ExistingResult(resultId, examId));
        _examinationRepo.SetupGetById(examId, ExistingExam(examId));
        SetupStaffQuery(Array.Empty<Staff>());

        var act = async () => await CreateHandler().Handle(Cmd(resultId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Current user is not registered as staff.");
    }

    [Fact]
    public async Task Overrides_result_and_records_metadata_when_command_is_valid()
    {
        var resultId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var existing = ExistingResult(resultId, examId);
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _examResultRepo.SetupGetById(resultId, existing);
        _examinationRepo.SetupGetById(examId, ExistingExam(examId, maxScore: 100, passScore: 50));
        SetupStaffQuery(new[] { new Staff { Id = staffId, UserAccountId = userId } });

        await CreateHandler().Handle(Cmd(resultId, score: 92m, overrideReason: "  Recount of marks  "), default);

        existing.Score.Should().Be(92m);
        existing.Grade.Should().Be("A");
        existing.IsPassed.Should().BeTrue();
        existing.IsFinalized.Should().BeTrue();
        existing.IsOverridden.Should().BeTrue();
        existing.OverrideById.Should().Be(staffId);
        existing.OverrideReason.Should().Be("Recount of marks");
        _examResultRepo.Verify(r => r.Update(existing), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Marks_result_as_failed_when_overridden_score_is_below_pass_score()
    {
        var resultId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existing = ExistingResult(resultId, examId);
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _examResultRepo.SetupGetById(resultId, existing);
        _examinationRepo.SetupGetById(examId, ExistingExam(examId, maxScore: 100, passScore: 50));
        SetupStaffQuery(new[] { new Staff { Id = Guid.NewGuid(), UserAccountId = userId } });

        await CreateHandler().Handle(Cmd(resultId, score: 20m, overrideReason: "Mistake correction"), default);

        existing.Score.Should().Be(20m);
        existing.Grade.Should().Be("F");
        existing.IsPassed.Should().BeFalse();
        existing.IsFinalized.Should().BeTrue();
        existing.IsOverridden.Should().BeTrue();
    }
}
