using System;
using FluentValidation;

namespace ZealEducation.Application.Features.SystemAssets.Commands.CreateSystemAsset;

public class CreateSystemAssetCommandValidator : AbstractValidator<CreateSystemAssetCommand>
{
    public CreateSystemAssetCommandValidator()
    {
        RuleFor(x => x.AssetName)
            .NotEmpty().WithMessage("Asset Name is required.");

        RuleFor(x => x.SerialNumber)
            .NotEmpty().WithMessage("Serial Number is required.");

        RuleFor(x => x.PurchaseDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Purchase Date cannot be in the future.");

        RuleFor(x => x.AssetType)
            .NotEmpty().WithMessage("Asset Type is required.");
            
        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");
            
        RuleFor(x => x.ManagedBy)
            .NotEmpty().WithMessage("Managed By (Staff Id) is required.");
    }
}
