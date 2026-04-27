using FluentValidation;

namespace ZealEducation.Application.Features.Candidates.Commands.UpdateCandidate;

public class UpdateCandidateCommandValidator : AbstractValidator<UpdateCandidateCommand>
{
    public UpdateCandidateCommandValidator()
    {
        RuleFor(x => x.CandidateId).NotEmpty();

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress()
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Length(10).WithMessage("Phone number must be 10 digits")
            .Matches(@"^\d+$").WithMessage("Only digits are allowed");

        RuleFor(x => x.EmergencyContact).MaximumLength(100);

        RuleFor(x => x.Status).IsInEnum();
    }
}
