using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.ExamResults.Commands.UpdateExamResult;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.ExamResults;

public class UpdateExamResultCommandHandlerTests
{
    private readonly Mock<IRepository<ExamResult>> _examResultRepo = new();
    private readonly Mock<IRepository<Examination>> _examinationRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private UpdateExamResultCommandHandler CreateHandler() =>
        new(_examResultRepo.Object, _examinationRepo.Object, _uow.Object);

    private static UpdateExamResultCommand Cmd(
        Guid resultId,
        decimal score = 80m,
        bool isFinalized = false) => new(resultId, score, isFinalized);

    private static ExamResult ExistingResult(
        Guid resultId,
        Guid examId,
        bool isOverridden = false,
        bool isFinalized = false) => new()
        {
            Id = resultId,
            ExamId = examId,
            EnrollmentId = Guid.NewGuid(),
            Score = 50m,
            Grade = "D",
            IsPassed = true,
            IsFinalized = isFinalized,
            IsOverridden = isOverridden,
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

    [Fact]
    public async Task Throws_NotFoundException_when_result_does_not_exist()
    {
        var resultId = Guid.NewGuid();
        _examResultRepo.SetupGetById(resultId, null);

        var act = async () => await CreateHandler().Handle(Cmd(resultId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _examResultRepo.Verify(r => r.Update(It.IsAny<ExamResult>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_result_is_overridden()
    {
        var resultId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        _examResultRepo.SetupGetById(resultId, ExistingResult(resultId, examId, isOverridden: true));

        var act = async () => await CreateHandler().Handle(Cmd(resultId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("This result has been overridden and can no longer be updated through this endpoint.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_result_is_finalized()
    {
        var resultId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        _examResultRepo.SetupGetById(resultId, ExistingResult(resultId, examId, isFinalized: true));

        var act = async () => await CreateHandler().Handle(Cmd(resultId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("This result has been finalized and can no longer be updated. Ask Incharge for an override.");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_examination_does_not_exist()
    {
        var resultId = Guid.NewGuid();
        var examId = Guid.NewGuid();
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
        _examResultRepo.SetupGetById(resultId, ExistingResult(resultId, examId));
        _examinationRepo.SetupGetById(examId, ExistingExam(examId, maxScore: 100));

        var act = async () => await CreateHandler().Handle(Cmd(resultId, score: 150m), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Score must not exceed the exam's max score (100).");
    }

    [Fact]
    public async Task Updates_result_fields_when_command_is_valid()
    {
        var resultId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var existing = ExistingResult(resultId, examId);
        _examResultRepo.SetupGetById(resultId, existing);
        _examinationRepo.SetupGetById(examId, ExistingExam(examId, maxScore: 100, passScore: 50));

        await CreateHandler().Handle(Cmd(resultId, score: 95m, isFinalized: true), default);

        existing.Score.Should().Be(95m);
        existing.Grade.Should().Be("A");
        existing.IsPassed.Should().BeTrue();
        existing.IsFinalized.Should().BeTrue();
        _examResultRepo.Verify(r => r.Update(existing), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Marks_result_as_failed_when_score_is_below_pass_score()
    {
        var resultId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var existing = ExistingResult(resultId, examId);
        _examResultRepo.SetupGetById(resultId, existing);
        _examinationRepo.SetupGetById(examId, ExistingExam(examId, maxScore: 100, passScore: 50));

        await CreateHandler().Handle(Cmd(resultId, score: 30m, isFinalized: false), default);

        existing.Score.Should().Be(30m);
        existing.Grade.Should().Be("F");
        existing.IsPassed.Should().BeFalse();
        existing.IsFinalized.Should().BeFalse();
        _examResultRepo.Verify(r => r.Update(existing), Times.Once);
    }
}
