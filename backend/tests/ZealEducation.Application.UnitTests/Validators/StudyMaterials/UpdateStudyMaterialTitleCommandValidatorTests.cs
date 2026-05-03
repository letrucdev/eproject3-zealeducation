using FluentValidation.TestHelper;
using ZealEducation.Application.Features.StudyMaterials.Commands.UpdateStudyMaterialTitle;

namespace ZealEducation.Application.UnitTests.Validators.StudyMaterials;

public class UpdateStudyMaterialTitleCommandValidatorTests
{
    private readonly UpdateStudyMaterialTitleCommandValidator _validator = new();

    private static UpdateStudyMaterialTitleCommand Valid(
        Guid? materialId = null,
        string title = "Updated Title") =>
        new(materialId ?? Guid.NewGuid(), title);

    [Fact]
    public void Should_pass_for_valid_command()
    {
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_material_id_is_empty()
    {
        _validator.TestValidate(Valid(materialId: Guid.Empty))
            .ShouldHaveValidationErrorFor(c => c.MaterialId);
    }

    [Fact]
    public void Should_fail_when_title_is_empty()
    {
        _validator.TestValidate(Valid(title: ""))
            .ShouldHaveValidationErrorFor(c => c.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Should_fail_when_title_exceeds_max_length()
    {
        _validator.TestValidate(Valid(title: new string('a', 201)))
            .ShouldHaveValidationErrorFor(c => c.Title);
    }
}
