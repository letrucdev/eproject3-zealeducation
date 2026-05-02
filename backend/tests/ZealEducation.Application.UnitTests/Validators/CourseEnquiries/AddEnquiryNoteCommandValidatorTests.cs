using FluentValidation.TestHelper;
using ZealEducation.Application.Features.CourseEnquiries.Commands.AddEnquiryNote;

namespace ZealEducation.Application.UnitTests.Validators.CourseEnquiries;

public class AddEnquiryNoteCommandValidatorTests
{
    private readonly AddEnquiryNoteCommandValidator _validator = new();

    private static AddEnquiryNoteCommand Valid(
        Guid? enquiryId = null,
        string content = "Called the candidate, no answer.") => new(
            enquiryId ?? Guid.NewGuid(),
            content);

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_enquiry_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(enquiryId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.EnquiryId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_fail_when_content_is_empty(string? content)
    {
        var result = _validator.TestValidate(Valid(content: content!));
        result.ShouldHaveValidationErrorFor(c => c.Content)
            .WithErrorMessage("Note content is required");
    }

    [Fact]
    public void Should_fail_when_content_exceeds_2000_chars()
    {
        var result = _validator.TestValidate(Valid(content: new string('x', 2001)));
        result.ShouldHaveValidationErrorFor(c => c.Content);
    }

    [Fact]
    public void Should_pass_when_content_is_at_max_length()
    {
        var result = _validator.TestValidate(Valid(content: new string('x', 2000)));
        result.ShouldNotHaveValidationErrorFor(c => c.Content);
    }
}
