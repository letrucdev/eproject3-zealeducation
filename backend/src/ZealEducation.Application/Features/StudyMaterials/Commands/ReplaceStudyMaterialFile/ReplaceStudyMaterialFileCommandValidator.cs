using FluentValidation;
using ZealEducation.Application.Features.StudyMaterials.Common;

namespace ZealEducation.Application.Features.StudyMaterials.Commands.ReplaceStudyMaterialFile;

public class ReplaceStudyMaterialFileCommandValidator : AbstractValidator<ReplaceStudyMaterialFileCommand>
{
    public ReplaceStudyMaterialFileCommandValidator()
    {
        RuleFor(x => x.MaterialId).NotEmpty();

        RuleFor(x => x.FileContent)
            .NotNull().WithMessage("A file is required.");

        RuleFor(x => x.FileLength)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaterialFileRules.MaxSizeBytes)
            .WithMessage($"File size must be between 1 byte and {MaterialFileRules.MaxSizeBytes / (1024 * 1024)} MB.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .Must(MaterialFileRules.IsAllowedExtension)
            .WithMessage($"File type is not allowed. Allowed: {string.Join(", ", MaterialFileRules.AllowedExtensions)}.");
    }
}
