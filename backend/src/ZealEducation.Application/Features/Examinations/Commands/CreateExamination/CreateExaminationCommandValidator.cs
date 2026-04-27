using FluentValidation;

namespace ZealEducation.Application.Features.Examinations.Commands.CreateExamination;

public class CreateExaminationCommandValidator : AbstractValidator<CreateExaminationCommand>
{
    public CreateExaminationCommandValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();

        RuleFor(x => x.ExamName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Location)
            .MaximumLength(100);

        RuleFor(x => x.MaxScore)
            .GreaterThan(0)
            .WithMessage("Max score must be greater than 0.");

        RuleFor(x => x.PassScore)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x)
            .Must(x => x.PassScore <= x.MaxScore)
            .WithMessage("Pass score must be less than or equal to max score.");
    }
}
