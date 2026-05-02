using FluentValidation;

namespace ZealEducation.Application.Features.ExamResults.Commands.CreateExamResult;

public class CreateExamResultCommandValidator : AbstractValidator<CreateExamResultCommand>
{
    public CreateExamResultCommandValidator()
    {
        RuleFor(x => x.ExaminationId).NotEmpty();
        RuleFor(x => x.EnrollmentId).NotEmpty();

        RuleFor(x => x.Score)
            .GreaterThanOrEqualTo(0);
    }
}
