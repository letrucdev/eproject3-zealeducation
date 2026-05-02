using FluentValidation;

namespace ZealEducation.Application.Features.Feedback.Commands.SubmitFacultyFeedback;

public class SubmitFacultyFeedbackCommandValidator : AbstractValidator<SubmitFacultyFeedbackCommand>
{
    public SubmitFacultyFeedbackCommandValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();
        RuleFor(x => x.FacultyId).NotEmpty();
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");
        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Comment must be 1000 characters or fewer.");
    }
}
