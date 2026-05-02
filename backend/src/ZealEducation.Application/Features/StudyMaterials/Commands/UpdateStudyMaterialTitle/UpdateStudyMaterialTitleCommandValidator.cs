using FluentValidation;

namespace ZealEducation.Application.Features.StudyMaterials.Commands.UpdateStudyMaterialTitle;

public class UpdateStudyMaterialTitleCommandValidator : AbstractValidator<UpdateStudyMaterialTitleCommand>
{
    public UpdateStudyMaterialTitleCommandValidator()
    {
        RuleFor(x => x.MaterialId).NotEmpty();
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);
    }
}
