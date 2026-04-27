using FluentValidation;

namespace ZealEducation.Application.Features.Examinations.Commands.UpdateExamination;

public class UpdateExaminationCommandValidator : AbstractValidator<UpdateExaminationCommand>
{
    public UpdateExaminationCommandValidator()
    {
        RuleFor(x => x.ExaminationId).NotEmpty();

        RuleFor(x => x.ExamName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Location)
            .MaximumLength(100);

        RuleFor(x => x.MaxScore)
            .GreaterThan(0);

        RuleFor(x => x.PassScore)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x)
            .Must(x => x.PassScore <= x.MaxScore)
            .WithMessage("Pass score must be less than or equal to max score.");
    }
}
