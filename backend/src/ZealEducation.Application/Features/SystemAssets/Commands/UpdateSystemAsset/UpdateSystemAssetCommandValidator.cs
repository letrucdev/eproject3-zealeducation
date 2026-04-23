using System;
using FluentValidation;

namespace ZealEducation.Application.Features.SystemAssets.Commands.UpdateSystemAsset;

public class UpdateSystemAssetCommandValidator : AbstractValidator<UpdateSystemAssetCommand>
{
    public UpdateSystemAssetCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.AssetName)
            .NotEmpty().WithMessage("Asset Name is required.");

        RuleFor(x => x.AssetType)
            .NotEmpty().WithMessage("Asset Type is required.");
            
        RuleFor(x => x.SerialNumber)
            .NotEmpty().WithMessage("Serial Number is required.");
            
        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");
    }
}
