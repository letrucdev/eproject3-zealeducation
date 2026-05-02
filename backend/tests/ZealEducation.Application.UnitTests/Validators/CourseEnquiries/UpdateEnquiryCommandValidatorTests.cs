using FluentValidation.TestHelper;
using ZealEducation.Application.Features.CourseEnquiries.Commands.UpdateEnquiry;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.UnitTests.Validators.CourseEnquiries;

public class UpdateEnquiryCommandValidatorTests
{
    private readonly UpdateEnquiryCommandValidator _validator = new();

    private static UpdateEnquiryCommand Valid(
        Guid? enquiryId = null,
        string fullName = "Jane Smith",
        string phone = "0123456789",
        string? email = "jane@example.com",
        Guid? courseId = null,
        EnquirySource source = EnquirySource.Phone,
        EnquiryStatus status = EnquiryStatus.Contacted,
        DateOnly? nextFollowUp = null) => new(
            enquiryId ?? Guid.NewGuid(),
            fullName,
            phone,
            email,
            courseId ?? Guid.NewGuid(),
            source,
            status,
            nextFollowUp);

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
    public void Should_fail_when_full_name_is_empty(string? name)
    {
        var result = _validator.TestValidate(Valid(fullName: name!));
        result.ShouldHaveValidationErrorFor(c => c.FullName);
    }

    [Fact]
    public void Should_fail_when_full_name_exceeds_100_chars()
    {
        var result = _validator.TestValidate(Valid(fullName: new string('A', 101)));
        result.ShouldHaveValidationErrorFor(c => c.FullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_fail_when_phone_is_empty(string? phone)
    {
        var result = _validator.TestValidate(Valid(phone: phone!));
        result.ShouldHaveValidationErrorFor(c => c.Phone);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("12345678901")]
    public void Should_fail_when_phone_length_is_not_10(string phone)
    {
        var result = _validator.TestValidate(Valid(phone: phone));
        result.ShouldHaveValidationErrorFor(c => c.Phone);
    }

    [Fact]
    public void Should_fail_when_phone_contains_non_digits()
    {
        var result = _validator.TestValidate(Valid(phone: "01234abcde"));
        result.ShouldHaveValidationErrorFor(c => c.Phone);
    }

    [Fact]
    public void Should_pass_when_email_is_null()
    {
        var result = _validator.TestValidate(Valid(email: null));
        result.ShouldNotHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Should_pass_when_email_is_whitespace()
    {
        var result = _validator.TestValidate(Valid(email: "   "));
        result.ShouldNotHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Should_fail_when_email_is_invalid_format()
    {
        var result = _validator.TestValidate(Valid(email: "not-an-email"));
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Should_fail_when_email_exceeds_100_chars()
    {
        var longEmail = new string('a', 96) + "@x.io";
        var result = _validator.TestValidate(Valid(email: longEmail));
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Should_fail_when_course_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(courseId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.CourseInterestedId);
    }

    [Fact]
    public void Should_fail_when_source_is_invalid_enum()
    {
        var result = _validator.TestValidate(Valid(source: (EnquirySource)999));
        result.ShouldHaveValidationErrorFor(c => c.Source);
    }

    [Fact]
    public void Should_fail_when_status_is_invalid_enum()
    {
        var result = _validator.TestValidate(Valid(status: (EnquiryStatus)999));
        result.ShouldHaveValidationErrorFor(c => c.Status);
    }

    [Fact]
    public void Should_fail_when_status_is_converted()
    {
        var result = _validator.TestValidate(Valid(status: EnquiryStatus.Converted));
        result.ShouldHaveValidationErrorFor(c => c.Status)
            .WithErrorMessage("Use the convert endpoint to mark an enquiry as Converted.");
    }

    [Fact]
    public void Should_pass_when_status_is_closed()
    {
        var result = _validator.TestValidate(Valid(status: EnquiryStatus.Closed));
        result.ShouldNotHaveValidationErrorFor(c => c.Status);
    }
}
