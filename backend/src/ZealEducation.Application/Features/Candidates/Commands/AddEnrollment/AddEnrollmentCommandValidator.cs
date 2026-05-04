using FluentValidation;

namespace ZealEducation.Application.Features.Candidates.Commands.AddEnrollment;

public class AddEnrollmentCommandValidator : AbstractValidator<AddEnrollmentCommand>
{
    public AddEnrollmentCommandValidator()
    {
        RuleFor(x => x.CandidateId).NotEmpty().WithMessage("Candidate is required.");
        RuleFor(x => x.CourseId).NotEmpty().WithMessage("Course is required.");
    }
}
