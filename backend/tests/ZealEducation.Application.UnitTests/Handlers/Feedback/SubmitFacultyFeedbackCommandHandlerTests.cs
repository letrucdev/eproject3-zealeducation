using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Feedback.Commands.SubmitFacultyFeedback;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Feedback;

public class SubmitFacultyFeedbackCommandHandlerTests
{
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IRepository<Candidate>> _candidateRepo = new();
    private readonly Mock<IRepository<Enrollment>> _enrollmentRepo = new();
    private readonly Mock<IRepository<Batch>> _batchRepo = new();
    private readonly Mock<IRepository<Domain.Entities.Feedback>> _feedbackRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private SubmitFacultyFeedbackCommandHandler CreateHandler() => new(
        _currentUser.Object,
        _candidateRepo.Object,
        _enrollmentRepo.Object,
        _batchRepo.Object,
        _feedbackRepo.Object,
        _uow.Object);

    private static SubmitFacultyFeedbackCommand Cmd(
        Guid? batchId = null,
        Guid? facultyId = null,
        int rating = 4,
        string? comment = "Helpful instructor") =>
        new(batchId ?? Guid.NewGuid(), facultyId ?? Guid.NewGuid(), rating, comment);

    private void SetupEnrollmentQuery(IEnumerable<Enrollment> data) =>
        _enrollmentRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Enrollment>(data));

    private void SetupBatchQuery(IEnumerable<Batch> data) =>
        _batchRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Batch>(data));

    private void SetupFeedbackQuery(IEnumerable<Domain.Entities.Feedback> data) =>
        _feedbackRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Domain.Entities.Feedback>(data));

    private static Batch BatchWith(Guid batchId, Guid? facultyId) => new()
    {
        Id = batchId,
        BatchCode = "B-001",
        CourseId = Guid.NewGuid(),
        FacultyId = facultyId,
        StartDate = new DateOnly(2030, 1, 1),
        EndDate = new DateOnly(2030, 6, 1),
        MaxCapacity = 30,
        Status = BatchStatus.Active
    };

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
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
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
    }

    [Fact]
    public async Task Throws_NotFoundException_when_batch_does_not_exist()
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
        SetupBatchQuery(Array.Empty<Batch>());

        var act = async () => await CreateHandler().Handle(Cmd(batchId: batchId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_ConflictException_when_faculty_does_not_teach_batch()
    {
        var userId = Guid.NewGuid();
        var candidate = new Candidate { Id = Guid.NewGuid(), UserAccountId = userId };
        var batchId = Guid.NewGuid();
        var requestedFacultyId = Guid.NewGuid();
        var actualFacultyId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _candidateRepo.SetupFind(new[] { candidate });
        SetupEnrollmentQuery(new[]
        {
            new Enrollment { Id = Guid.NewGuid(), CandidateId = candidate.Id, BatchId = batchId, CourseId = Guid.NewGuid() }
        });
        SetupBatchQuery(new[] { BatchWith(batchId, actualFacultyId) });

        var act = async () => await CreateHandler().Handle(Cmd(batchId: batchId, facultyId: requestedFacultyId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("This faculty does not teach the selected batch.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_faculty_feedback_already_submitted()
    {
        var userId = Guid.NewGuid();
        var candidate = new Candidate { Id = Guid.NewGuid(), UserAccountId = userId };
        var batchId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _candidateRepo.SetupFind(new[] { candidate });
        SetupEnrollmentQuery(new[]
        {
            new Enrollment { Id = Guid.NewGuid(), CandidateId = candidate.Id, BatchId = batchId, CourseId = Guid.NewGuid() }
        });
        SetupBatchQuery(new[] { BatchWith(batchId, facultyId) });
        SetupFeedbackQuery(new[]
        {
            new Domain.Entities.Feedback
            {
                Id = Guid.NewGuid(),
                CandidateId = candidate.Id,
                BatchId = batchId,
                Type = FeedbackType.Faculty,
                TargetFacultyId = facultyId,
                Rating = 4
            }
        });

        var act = async () => await CreateHandler().Handle(Cmd(batchId: batchId, facultyId: facultyId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("You have already submitted feedback for this faculty.");
        _feedbackRepo.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Feedback>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Submits_faculty_feedback_when_valid()
    {
        var userId = Guid.NewGuid();
        var candidate = new Candidate { Id = Guid.NewGuid(), UserAccountId = userId };
        var batchId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _candidateRepo.SetupFind(new[] { candidate });
        SetupEnrollmentQuery(new[]
        {
            new Enrollment { Id = Guid.NewGuid(), CandidateId = candidate.Id, BatchId = batchId, CourseId = Guid.NewGuid() }
        });
        SetupBatchQuery(new[] { BatchWith(batchId, facultyId) });
        SetupFeedbackQuery(Array.Empty<Domain.Entities.Feedback>());

        Domain.Entities.Feedback? captured = null;
        _feedbackRepo.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Feedback>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.Feedback, CancellationToken>((f, _) => captured = f)
            .ReturnsAsync((Domain.Entities.Feedback f, CancellationToken _) => f);

        var result = await CreateHandler().Handle(
            Cmd(batchId: batchId, facultyId: facultyId, rating: 5, comment: "  Top instructor  "),
            default);

        captured.Should().NotBeNull();
        captured!.CandidateId.Should().Be(candidate.Id);
        captured.BatchId.Should().Be(batchId);
        captured.Type.Should().Be(FeedbackType.Faculty);
        captured.TargetFacultyId.Should().Be(facultyId);
        captured.Rating.Should().Be(5);
        captured.Comment.Should().Be("Top instructor");
        captured.IsProcessed.Should().BeFalse();
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
        var facultyId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _candidateRepo.SetupFind(new[] { candidate });
        SetupEnrollmentQuery(new[]
        {
            new Enrollment { Id = Guid.NewGuid(), CandidateId = candidate.Id, BatchId = batchId, CourseId = Guid.NewGuid() }
        });
        SetupBatchQuery(new[] { BatchWith(batchId, facultyId) });
        SetupFeedbackQuery(Array.Empty<Domain.Entities.Feedback>());

        Domain.Entities.Feedback? captured = null;
        _feedbackRepo.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Feedback>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.Feedback, CancellationToken>((f, _) => captured = f)
            .ReturnsAsync((Domain.Entities.Feedback f, CancellationToken _) => f);

        await CreateHandler().Handle(Cmd(batchId: batchId, facultyId: facultyId, rating: 2, comment: "   "), default);

        captured!.Comment.Should().BeNull();
    }
}
