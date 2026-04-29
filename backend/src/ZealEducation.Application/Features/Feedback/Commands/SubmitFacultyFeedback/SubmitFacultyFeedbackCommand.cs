using MediatR;

namespace ZealEducation.Application.Features.Feedback.Commands.SubmitFacultyFeedback;

public record SubmitFacultyFeedbackCommand(
    Guid BatchId,
    Guid FacultyId,
    int Rating,
    string? Comment) : IRequest<Guid>;
