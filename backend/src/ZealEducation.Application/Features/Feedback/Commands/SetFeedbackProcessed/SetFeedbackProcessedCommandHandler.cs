using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Feedback.Commands.SetFeedbackProcessed;

public class SetFeedbackProcessedCommandHandler(
    IRepository<Domain.Entities.Feedback> feedbackRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<SetFeedbackProcessedCommand, Unit>
{
    public async Task<Unit> Handle(SetFeedbackProcessedCommand request, CancellationToken cancellationToken)
    {
        var feedback = await feedbackRepository.GetByIdAsync(request.FeedbackId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Feedback), request.FeedbackId);

        // Idempotent — re-confirming the same state should not churn ProcessedAt.
        if (feedback.IsProcessed == request.IsProcessed)
            return Unit.Value;

        feedback.IsProcessed = request.IsProcessed;
        if (request.IsProcessed)
        {
            var userId = currentUser.UserId
                ?? throw new UnauthorizedException("Current user could not be resolved.");
            feedback.ProcessedById = userId;
            feedback.ProcessedAt = DateTime.UtcNow;
        }
        else
        {
            feedback.ProcessedById = null;
            feedback.ProcessedAt = null;
        }

        feedbackRepository.Update(feedback);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
