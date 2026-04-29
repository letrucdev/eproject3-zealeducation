using MediatR;

namespace ZealEducation.Application.Features.Feedback.Commands.SubmitCourseFeedback;

public record SubmitCourseFeedbackCommand(
    Guid BatchId,
    int Rating,
    string? Comment) : IRequest<Guid>;
