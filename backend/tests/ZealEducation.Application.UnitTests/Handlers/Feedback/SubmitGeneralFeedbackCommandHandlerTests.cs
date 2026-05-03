using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Feedback.Commands.SubmitGeneralFeedback;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Feedback;

public class SubmitGeneralFeedbackCommandHandlerTests
{
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IRepository<Candidate>> _candidateRepo = new();
    private readonly Mock<IRepository<Enrollment>> _enrollmentRepo = new();
    private readonly Mock<IRepository<Domain.Entities.Feedback>> _feedbackRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private SubmitGeneralFeedbackCommandHandler CreateHandler() => new(
        _currentUser.Object,
        _candidateRepo.Object,
        _enrollmentRepo.Object,
        _feedbackRepo.Object,
        _uow.Object);

    private static SubmitGeneralFeedbackCommand Cmd(
        Guid? batchId = null,
        int rating = 5,
        string? comment = "Excellent batch!") =>
        new(batchId ?? Guid.NewGuid(), rating, comment);

    private void SetupEnrollmentQuery(IEnumerable<Enrollment> data) =>
        _enrollmentRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Enrollment>(data));

    private void SetupFeedbackQuery(IEnumerable<Domain.Entities.Feedback> data) =>
        _feedbackRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Domain.Entities.Feedback>(data));

    [Fact]
    public async Task Throws_UnauthorizedException_when_current_user_is_missing()
    {
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_no_candidate_profile()
    {
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _candidateRepo.SetupFind(Array.Empty<Candidate>());

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_NotFoundException_when_not_enrolled_in_batch()
    {
        var userId = Guid.NewGuid();
        var candidate = new Candidate { Id = Guid.NewGuid(), UserAccountId = userId };
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _candidateRepo.SetupFind(new[] { candidate });
        SetupEnrollmentQuery(Array.Empty<Enrollment>());

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("You are not enrolled in this batch.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_general_feedback_already_submitted()
    {
        var userId = Guid.NewGuid();
        var candidate = new Candidate { Id = Guid.NewGuid(), UserAccountId = userId };
        var batchId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _candidateRepo.SetupFind(new[] { candidate });
        SetupEnrollmentQuery(new[]
        {
            new Enrollment { Id = Guid.NewGuid(), CandidateId = candidate.Id, BatchId = batchId, CourseId = Guid.NewGuid() }
        });
        SetupFeedbackQuery(new[]
        {
            new Domain.Entities.Feedback
            {
                Id = Guid.NewGuid(),
                CandidateId = candidate.Id,
                BatchId = batchId,
                Type = FeedbackType.General,
                Rating = 4
            }
        });

        var act = async () => await CreateHandler().Handle(Cmd(batchId: batchId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("You have already submitted general feedback for this batch.");
        _feedbackRepo.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Feedback>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Submits_feedback_and_persists_when_valid()
    {
        var userId = Guid.NewGuid();
        var candidate = new Candidate { Id = Guid.NewGuid(), UserAccountId = userId };
        var batchId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _candidateRepo.SetupFind(new[] { candidate });
        SetupEnrollmentQuery(new[]
        {
            new Enrollment { Id = Guid.NewGuid(), CandidateId = candidate.Id, BatchId = batchId, CourseId = Guid.NewGuid() }
        });
        SetupFeedbackQuery(Array.Empty<Domain.Entities.Feedback>());

        Domain.Entities.Feedback? captured = null;
        _feedbackRepo.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Feedback>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.Feedback, CancellationToken>((f, _) => captured = f)
            .ReturnsAsync((Domain.Entities.Feedback f, CancellationToken _) => f);

        var result = await CreateHandler().Handle(Cmd(batchId: batchId, rating: 5, comment: "  Great!  "), default);

        captured.Should().NotBeNull();
        captured!.CandidateId.Should().Be(candidate.Id);
        captured.BatchId.Should().Be(batchId);
        captured.Type.Should().Be(FeedbackType.General);
        captured.Rating.Should().Be(5);
        captured.Comment.Should().Be("Great!");
        captured.IsProcessed.Should().BeFalse();
        captured.TargetFacultyId.Should().BeNull();
        captured.Id.Should().NotBe(Guid.Empty);
        result.Should().Be(captured.Id);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Normalizes_blank_comment_to_null()
    {
        var userId = Guid.NewGuid();
        var candidate = new Candidate { Id = Guid.NewGuid(), UserAccountId = userId };
        var batchId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _candidateRepo.SetupFind(new[] { candidate });
        SetupEnrollmentQuery(new[]
        {
            new Enrollment { Id = Guid.NewGuid(), CandidateId = candidate.Id, BatchId = batchId, CourseId = Guid.NewGuid() }
        });
        SetupFeedbackQuery(Array.Empty<Domain.Entities.Feedback>());

        Domain.Entities.Feedback? captured = null;
        _feedbackRepo.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Feedback>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.Feedback, CancellationToken>((f, _) => captured = f)
            .ReturnsAsync((Domain.Entities.Feedback f, CancellationToken _) => f);

        await CreateHandler().Handle(Cmd(batchId: batchId, rating: 3, comment: "   "), default);

        captured!.Comment.Should().BeNull();
    }
}
