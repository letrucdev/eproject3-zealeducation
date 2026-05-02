using FluentValidation.TestHelper;
using ZealEducation.Application.Features.StudyMaterials.Commands.ReplaceStudyMaterialFile;
using ZealEducation.Application.Features.StudyMaterials.Common;

namespace ZealEducation.Application.UnitTests.Validators.StudyMaterials;

public class ReplaceStudyMaterialFileCommandValidatorTests
{
    private readonly ReplaceStudyMaterialFileCommandValidator _validator = new();

    private static ReplaceStudyMaterialFileCommand Valid(
        Guid? materialId = null,
        Stream? content = null,
        string contentType = "application/pdf",
        string fileName = "replacement.pdf",
        long fileLength = 2048) => new(
            materialId ?? Guid.NewGuid(),
            content ?? new MemoryStream(new byte[] { 1, 2, 3 }),
            contentType,
            fileName,
            fileLength);

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
    public void Should_fail_when_file_content_is_null()
    {
        var cmd = new ReplaceStudyMaterialFileCommand(
            Guid.NewGuid(),
            null!,
            "application/pdf",
            "replacement.pdf",
            2048);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.FileContent);
    }

    [Fact]
    public void Should_fail_when_file_length_is_zero()
    {
        _validator.TestValidate(Valid(fileLength: 0))
            .ShouldHaveValidationErrorFor(c => c.FileLength);
    }

    [Fact]
    public void Should_fail_when_file_length_exceeds_max()
    {
        _validator.TestValidate(Valid(fileLength: MaterialFileRules.MaxSizeBytes + 1))
            .ShouldHaveValidationErrorFor(c => c.FileLength);
    }

    [Fact]
    public void Should_fail_when_file_name_is_empty()
    {
        _validator.TestValidate(Valid(fileName: ""))
            .ShouldHaveValidationErrorFor(c => c.FileName)
            .WithErrorMessage("File name is required.");
    }

    [Fact]
    public void Should_fail_when_file_extension_is_unsupported()
    {
        _validator.TestValidate(Valid(fileName: "archive.zip"))
            .ShouldHaveValidationErrorFor(c => c.FileName);
    }

    [Fact]
    public void Should_pass_for_each_allowed_extension()
    {
        foreach (var ext in MaterialFileRules.AllowedExtensions)
        {
            _validator.TestValidate(Valid(fileName: $"file{ext}"))
                .ShouldNotHaveValidationErrorFor(c => c.FileName);
        }
    }
}
