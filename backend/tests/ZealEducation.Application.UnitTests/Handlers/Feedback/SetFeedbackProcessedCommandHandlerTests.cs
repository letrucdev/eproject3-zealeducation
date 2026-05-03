using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Feedback.Commands.SetFeedbackProcessed;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Feedback;

public class SetFeedbackProcessedCommandHandlerTests
{
    private readonly Mock<IRepository<Domain.Entities.Feedback>> _feedbackRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private SetFeedbackProcessedCommandHandler CreateHandler() => new(
        _feedbackRepo.Object,
        _currentUser.Object,
        _uow.Object);

    private static Domain.Entities.Feedback ExistingFeedback(
        Guid id,
        bool isProcessed = false,
        Guid? processedById = null,
        DateTime? processedAt = null) => new()
    {
        Id = id,
        CandidateId = Guid.NewGuid(),
        BatchId = Guid.NewGuid(),
        Type = FeedbackType.General,
        Rating = 4,
        IsProcessed = isProcessed,
        ProcessedById = processedById,
        ProcessedAt = processedAt
    };

    [Fact]
    public async Task Throws_NotFoundException_when_feedback_does_not_exist()
    {
        var feedbackId = Guid.NewGuid();
        _feedbackRepo.SetupGetById(feedbackId, null);

        var act = async () => await CreateHandler().Handle(
            new SetFeedbackProcessedCommand(feedbackId, true), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _feedbackRepo.Verify(r => r.Update(It.IsAny<Domain.Entities.Feedback>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Returns_unit_without_changes_when_state_is_already_processed()
    {
        var feedbackId = Guid.NewGuid();
        var processedById = Guid.NewGuid();
        var processedAt = DateTime.UtcNow.AddDays(-1);
        var feedback = ExistingFeedback(feedbackId, isProcessed: true, processedById: processedById, processedAt: processedAt);
        _feedbackRepo.SetupGetById(feedbackId, feedback);

        await CreateHandler().Handle(new SetFeedbackProcessedCommand(feedbackId, true), default);

        feedback.IsProcessed.Should().BeTrue();
        feedback.ProcessedById.Should().Be(processedById);
        feedback.ProcessedAt.Should().Be(processedAt);
        _feedbackRepo.Verify(r => r.Update(It.IsAny<Domain.Entities.Feedback>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Returns_unit_without_changes_when_state_is_already_unprocessed()
    {
        var feedbackId = Guid.NewGuid();
        var feedback = ExistingFeedback(feedbackId, isProcessed: false);
        _feedbackRepo.SetupGetById(feedbackId, feedback);

        await CreateHandler().Handle(new SetFeedbackProcessedCommand(feedbackId, false), default);

        feedback.IsProcessed.Should().BeFalse();
        _feedbackRepo.Verify(r => r.Update(It.IsAny<Domain.Entities.Feedback>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_UnauthorizedException_when_marking_processed_without_current_user()
    {
        var feedbackId = Guid.NewGuid();
        var feedback = ExistingFeedback(feedbackId, isProcessed: false);
        _feedbackRepo.SetupGetById(feedbackId, feedback);
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(
            new SetFeedbackProcessedCommand(feedbackId, true), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _feedbackRepo.Verify(r => r.Update(It.IsAny<Domain.Entities.Feedback>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Marks_feedback_as_processed_and_records_processor()
    {
        var feedbackId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var feedback = ExistingFeedback(feedbackId, isProcessed: false);
        _feedbackRepo.SetupGetById(feedbackId, feedback);
        _currentUser.SetupGet(u => u.UserId).Returns(userId);

        var before = DateTime.UtcNow;
        await CreateHandler().Handle(new SetFeedbackProcessedCommand(feedbackId, true), default);
        var after = DateTime.UtcNow;

        feedback.IsProcessed.Should().BeTrue();
        feedback.ProcessedById.Should().Be(userId);
        feedback.ProcessedAt.Should().NotBeNull();
        feedback.ProcessedAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        _feedbackRepo.Verify(r => r.Update(feedback), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Resets_processed_metadata_when_marking_unprocessed()
    {
        var feedbackId = Guid.NewGuid();
        var feedback = ExistingFeedback(
            feedbackId,
            isProcessed: true,
            processedById: Guid.NewGuid(),
            processedAt: DateTime.UtcNow.AddDays(-2));
        _feedbackRepo.SetupGetById(feedbackId, feedback);

        await CreateHandler().Handle(new SetFeedbackProcessedCommand(feedbackId, false), default);

        feedback.IsProcessed.Should().BeFalse();
        feedback.ProcessedById.Should().BeNull();
        feedback.ProcessedAt.Should().BeNull();
        _feedbackRepo.Verify(r => r.Update(feedback), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
