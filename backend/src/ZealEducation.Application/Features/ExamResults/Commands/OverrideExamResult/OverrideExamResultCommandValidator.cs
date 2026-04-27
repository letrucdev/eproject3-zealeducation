using FluentValidation;

namespace ZealEducation.Application.Features.ExamResults.Commands.OverrideExamResult;

public class OverrideExamResultCommandValidator : AbstractValidator<OverrideExamResultCommand>
{
    public OverrideExamResultCommandValidator()
    {
        RuleFor(x => x.ResultId).NotEmpty();
        RuleFor(x => x.Score).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Grade).MaximumLength(5);

        RuleFor(x => x.OverrideReason)
            .NotEmpty().WithMessage("Override reason is required.")
            .MinimumLength(5).WithMessage("Override reason must be at least 5 characters.")
            .MaximumLength(500);
    }
}
