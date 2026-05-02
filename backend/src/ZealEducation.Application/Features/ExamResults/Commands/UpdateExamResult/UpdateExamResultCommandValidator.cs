using FluentValidation;

namespace ZealEducation.Application.Features.ExamResults.Commands.UpdateExamResult;

public class UpdateExamResultCommandValidator : AbstractValidator<UpdateExamResultCommand>
{
    public UpdateExamResultCommandValidator()
    {
        RuleFor(x => x.ResultId).NotEmpty();
        RuleFor(x => x.Score).GreaterThanOrEqualTo(0);
    }
}
