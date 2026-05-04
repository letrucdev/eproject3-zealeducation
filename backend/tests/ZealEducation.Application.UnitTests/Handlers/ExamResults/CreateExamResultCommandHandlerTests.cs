using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.ExamResults.Commands.CreateExamResult;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.ExamResults;

public class CreateExamResultCommandHandlerTests
{
    private readonly Mock<IRepository<Examination>> _examinationRepo = new();
    private readonly Mock<IRepository<Enrollment>> _enrollmentRepo = new();
    private readonly Mock<IRepository<ExamResult>> _examResultRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private CreateExamResultCommandHandler CreateHandler() => new(
        _examinationRepo.Object,
        _enrollmentRepo.Object,
        _examResultRepo.Object,
        _staffRepo.Object,
        _currentUser.Object,
        _uow.Object);

    private static CreateExamResultCommand Cmd(
        Guid examinationId,
        Guid enrollmentId,
        decimal score = 80m,
        bool isFinalized = false) => new(examinationId, enrollmentId, score, isFinalized);

    private static Examination ExistingExam(Guid examId, Guid batchId, int maxScore = 100, int passScore = 50) => new()
    {
        Id = examId,
        BatchId = batchId,
        ExamName = "Midterm",
        ExamDate = new DateOnly(2030, 3, 15),
        MaxScore = maxScore,
        PassScore = passScore,
        ScheduledById = Guid.NewGuid()
    };

    private static Enrollment ExistingEnrollment(Guid enrollmentId, Guid? batchId) => new()
    {
        Id = enrollmentId,
        BatchId = batchId,
        CandidateId = Guid.NewGuid(),
        CourseId = Guid.NewGuid()
    };

    private void SetupExamResultQuery(IEnumerable<ExamResult> data)
    {
        var queryable = new TestAsyncEnumerable<ExamResult>(data);
        _examResultRepo.Setup(r => r.Query()).Returns(queryable);
    }

    private void SetupStaffQuery(IEnumerable<Staff> data)
    {
        var queryable = new TestAsyncEnumerable<Staff>(data);
        _staffRepo.Setup(r => r.Query()).Returns(queryable);
    }

    [Fact]
    public async Task Throws_UnauthorizedException_when_current_user_is_not_resolved()
    {
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(
            Cmd(Guid.NewGuid(), Guid.NewGuid()), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _examResultRepo.Verify(r => r.AddAsync(It.IsAny<ExamResult>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_examination_does_not_exist()
    {
        var examId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _examinationRepo.SetupGetById(examId, null);

        var act = async () => await CreateHandler().Handle(
            Cmd(examId, Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_score_exceeds_max_score()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _examinationRepo.SetupGetById(examId, ExistingExam(examId, batchId, maxScore: 100));

        var act = async () => await CreateHandler().Handle(
            Cmd(examId, Guid.NewGuid(), score: 101m), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Score must not exceed the exam's max score (100).");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_enrollment_does_not_exist()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _examinationRepo.SetupGetById(examId, ExistingExam(examId, batchId));
        _enrollmentRepo.SetupGetById(enrollmentId, null);

        var act = async () => await CreateHandler().Handle(
            Cmd(examId, enrollmentId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_ConflictException_when_enrollment_batch_differs_from_exam_batch()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _examinationRepo.SetupGetById(examId, ExistingExam(examId, batchId));
        _enrollmentRepo.SetupGetById(enrollmentId, ExistingEnrollment(enrollmentId, batchId: Guid.NewGuid()));

        var act = async () => await CreateHandler().Handle(
            Cmd(examId, enrollmentId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("The enrollment does not belong to the same batch as the examination.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_result_already_exists_for_candidate_and_exam()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _examinationRepo.SetupGetById(examId, ExistingExam(examId, batchId));
        _enrollmentRepo.SetupGetById(enrollmentId, ExistingEnrollment(enrollmentId, batchId));
        SetupExamResultQuery(new[]
        {
            new ExamResult { Id = Guid.NewGuid(), ExamId = examId, EnrollmentId = enrollmentId }
        });

        var act = async () => await CreateHandler().Handle(
            Cmd(examId, enrollmentId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("A result already exists for this candidate on this examination.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_current_user_is_not_a_staff()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _examinationRepo.SetupGetById(examId, ExistingExam(examId, batchId));
        _enrollmentRepo.SetupGetById(enrollmentId, ExistingEnrollment(enrollmentId, batchId));
        SetupExamResultQuery(Array.Empty<ExamResult>());
        SetupStaffQuery(Array.Empty<Staff>());

        var act = async () => await CreateHandler().Handle(
            Cmd(examId, enrollmentId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Current user is not registered as staff.");
    }

    [Fact]
    public async Task Creates_exam_result_and_returns_id_when_command_is_valid()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _examinationRepo.SetupGetById(examId, ExistingExam(examId, batchId, maxScore: 100, passScore: 50));
        _enrollmentRepo.SetupGetById(enrollmentId, ExistingEnrollment(enrollmentId, batchId));
        SetupExamResultQuery(Array.Empty<ExamResult>());
        SetupStaffQuery(new[] { new Staff { Id = staffId, UserAccountId = userId } });

        ExamResult? captured = null;
        _examResultRepo.Setup(r => r.AddAsync(It.IsAny<ExamResult>(), It.IsAny<CancellationToken>()))
            .Callback<ExamResult, CancellationToken>((e, _) => captured = e)
            .ReturnsAsync((ExamResult e, CancellationToken _) => e);

        var resultId = await CreateHandler().Handle(
            Cmd(examId, enrollmentId, score: 85m, isFinalized: true), default);

        resultId.Should().NotBe(Guid.Empty);
        captured.Should().NotBeNull();
        captured!.Id.Should().Be(resultId);
        captured.ExamId.Should().Be(examId);
        captured.EnrollmentId.Should().Be(enrollmentId);
        captured.Score.Should().Be(85m);
        captured.Grade.Should().Be("B");
        captured.IsPassed.Should().BeTrue();
        captured.IsFinalized.Should().BeTrue();
        captured.GradedById.Should().Be(staffId);
        captured.IsOverridden.Should().BeFalse();
        captured.OverrideById.Should().BeNull();
        captured.GradedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Marks_result_as_failed_when_score_is_below_pass_score()
    {
        var examId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _examinationRepo.SetupGetById(examId, ExistingExam(examId, batchId, maxScore: 100, passScore: 50));
        _enrollmentRepo.SetupGetById(enrollmentId, ExistingEnrollment(enrollmentId, batchId));
        SetupExamResultQuery(Array.Empty<ExamResult>());
        SetupStaffQuery(new[] { new Staff { Id = Guid.NewGuid(), UserAccountId = userId } });

        ExamResult? captured = null;
        _examResultRepo.Setup(r => r.AddAsync(It.IsAny<ExamResult>(), It.IsAny<CancellationToken>()))
            .Callback<ExamResult, CancellationToken>((e, _) => captured = e)
            .ReturnsAsync((ExamResult e, CancellationToken _) => e);

        await CreateHandler().Handle(Cmd(examId, enrollmentId, score: 40m), default);

        captured!.IsPassed.Should().BeFalse();
        captured.Grade.Should().Be("F");
    }
}
