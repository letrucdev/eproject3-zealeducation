using FluentValidation;
using ZealEducation.Application.Features.StudyMaterials.Common;

namespace ZealEducation.Application.Features.StudyMaterials.Commands.CreateStudyMaterials;

public class CreateStudyMaterialsCommandValidator : AbstractValidator<CreateStudyMaterialsCommand>
{
    public CreateStudyMaterialsCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.CourseIds)
            .NotNull().WithMessage("At least one course must be selected.")
            .Must(ids => ids != null && ids.Count > 0)
            .WithMessage("At least one course must be selected.")
            .Must(ids => ids == null || ids.Count <= 50)
            .WithMessage("Cannot assign a single material to more than 50 courses at once.");

        RuleForEach(x => x.CourseIds)
            .Must(id => id != Guid.Empty)
            .WithMessage("Course id is invalid.");

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
