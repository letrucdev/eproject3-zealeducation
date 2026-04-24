using FluentValidation;

namespace ZealEducation.Application.Features.SystemAssets.Commands.DecommissionAsset;

public class DecommissionAssetCommandValidator : AbstractValidator<DecommissionAssetCommand>
{
    public DecommissionAssetCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}
