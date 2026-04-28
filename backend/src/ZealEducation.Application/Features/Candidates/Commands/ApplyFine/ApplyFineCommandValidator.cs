using FluentValidation;

namespace ZealEducation.Application.Features.Candidates.Commands.ApplyFine;

public class ApplyFineCommandValidator : AbstractValidator<ApplyFineCommand>
{
    public ApplyFineCommandValidator()
    {
        RuleFor(x => x.CandidateId).NotEmpty();

        RuleFor(x => x.ViolationReason)
            .NotEmpty().WithMessage("Violation reason is required")
            .MaximumLength(500);

        RuleFor(x => x.PenaltyAmount)
            .GreaterThan(0m).WithMessage("Penalty amount must be greater than 0");
    }
}
