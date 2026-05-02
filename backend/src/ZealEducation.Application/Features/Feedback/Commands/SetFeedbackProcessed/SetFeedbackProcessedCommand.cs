using MediatR;

namespace ZealEducation.Application.Features.Feedback.Commands.SetFeedbackProcessed;

public record SetFeedbackProcessedCommand(Guid FeedbackId, bool IsProcessed) : IRequest<Unit>;
