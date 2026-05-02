using MediatR;

namespace ZealEducation.Application.Features.Feedback.Commands.SubmitGeneralFeedback;

public record SubmitGeneralFeedbackCommand(
    Guid BatchId,
    int Rating,
    string? Comment) : IRequest<Guid>;
