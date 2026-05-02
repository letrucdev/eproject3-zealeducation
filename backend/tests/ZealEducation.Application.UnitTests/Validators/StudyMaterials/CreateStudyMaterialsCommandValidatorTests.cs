using FluentValidation.TestHelper;
using ZealEducation.Application.Features.StudyMaterials.Commands.CreateStudyMaterials;
using ZealEducation.Application.Features.StudyMaterials.Common;

namespace ZealEducation.Application.UnitTests.Validators.StudyMaterials;

public class CreateStudyMaterialsCommandValidatorTests
{
    private readonly CreateStudyMaterialsCommandValidator _validator = new();

    private static CreateStudyMaterialsCommand Valid(
        string title = "Lecture Notes",
        IReadOnlyList<Guid>? courseIds = null,
        Stream? content = null,
        string contentType = "application/pdf",
        string fileName = "notes.pdf",
        long fileLength = 1024) => new(
            title,
            courseIds ?? new[] { Guid.NewGuid() },
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

    [Fact]
    public void Should_fail_when_course_ids_is_null()
    {
        var cmd = new CreateStudyMaterialsCommand(
            "Lecture Notes",
            null!,
            new MemoryStream(new byte[] { 1, 2, 3 }),
            "application/pdf",
            "notes.pdf",
            1024);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.CourseIds);
    }

    [Fact]
    public void Should_fail_when_course_ids_is_empty()
    {
        _validator.TestValidate(Valid(courseIds: Array.Empty<Guid>()))
            .ShouldHaveValidationErrorFor(c => c.CourseIds)
            .WithErrorMessage("At least one course must be selected.");
    }

    [Fact]
    public void Should_fail_when_course_ids_exceeds_50()
    {
        var ids = Enumerable.Range(0, 51).Select(_ => Guid.NewGuid()).ToList();
        _validator.TestValidate(Valid(courseIds: ids))
            .ShouldHaveValidationErrorFor(c => c.CourseIds)
            .WithErrorMessage("Cannot assign a single material to more than 50 courses at once.");
    }

    [Fact]
    public void Should_fail_when_any_course_id_is_empty_guid()
    {
        _validator.TestValidate(Valid(courseIds: new[] { Guid.Empty }))
            .ShouldHaveValidationErrorFor("CourseIds[0]");
    }

    [Fact]
    public void Should_fail_when_file_content_is_null()
    {
        var cmd = new CreateStudyMaterialsCommand(
            "Lecture Notes",
            new[] { Guid.NewGuid() },
            null!,
            "application/pdf",
            "notes.pdf",
            1024);
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
        _validator.TestValidate(Valid(fileName: "malware.exe"))
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
