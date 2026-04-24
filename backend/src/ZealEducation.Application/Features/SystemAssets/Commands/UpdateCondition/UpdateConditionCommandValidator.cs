using FluentValidation;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.SystemAssets.Commands.UpdateCondition;

public class UpdateConditionCommandValidator : AbstractValidator<UpdateConditionCommand>
{
    public UpdateConditionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.ConditionStatus)
            .IsInEnum().WithMessage("Condition Status is invalid.");
    }
}
