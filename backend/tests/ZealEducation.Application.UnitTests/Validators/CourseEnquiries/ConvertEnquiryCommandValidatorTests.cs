using FluentValidation.TestHelper;
using ZealEducation.Application.Features.CourseEnquiries.Commands.ConvertEnquiry;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.UnitTests.Validators.CourseEnquiries;

public class ConvertEnquiryCommandValidatorTests
{
    private readonly ConvertEnquiryCommandValidator _validator = new();

    private static ConvertEnquiryCommand Valid(
        Guid? enquiryId = null,
        string email = "convert@example.com",
        DateOnly? dob = null,
        Gender gender = Gender.Female,
        string? address = "Some street 1",
        string? emergency = "0123456789") => new(
            enquiryId ?? Guid.NewGuid(),
            email,
            dob ?? new DateOnly(1995, 5, 5),
            gender,
            address,
            emergency);

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
    [InlineData(null)]
    public void Should_fail_when_email_is_empty(string? email)
    {
        var result = _validator.TestValidate(Valid(email: email!));
        result.ShouldHaveValidationErrorFor(c => c.Email)
            .WithErrorMessage("Email is required to send login information");
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
    public void Should_fail_when_dob_is_default()
    {
        var result = _validator.TestValidate(Valid(dob: default(DateOnly)));
        result.ShouldHaveValidationErrorFor(c => c.Dob);
    }

    [Fact]
    public void Should_fail_when_dob_is_today_or_future()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = _validator.TestValidate(Valid(dob: today));
        result.ShouldHaveValidationErrorFor(c => c.Dob)
            .WithErrorMessage("Date of birth must be in the past");

        var future = today.AddDays(1);
        result = _validator.TestValidate(Valid(dob: future));
        result.ShouldHaveValidationErrorFor(c => c.Dob);
    }

    [Fact]
    public void Should_fail_when_gender_is_invalid_enum()
    {
        var result = _validator.TestValidate(Valid(gender: (Gender)999));
        result.ShouldHaveValidationErrorFor(c => c.Gender);
    }

    [Fact]
    public void Should_fail_when_address_exceeds_500_chars()
    {
        var result = _validator.TestValidate(Valid(address: new string('a', 501)));
        result.ShouldHaveValidationErrorFor(c => c.Address);
    }

    [Fact]
    public void Should_pass_when_address_is_null()
    {
        var result = _validator.TestValidate(Valid(address: null));
        result.ShouldNotHaveValidationErrorFor(c => c.Address);
    }

    [Fact]
    public void Should_fail_when_emergency_contact_exceeds_100_chars()
    {
        var result = _validator.TestValidate(Valid(emergency: new string('a', 101)));
        result.ShouldHaveValidationErrorFor(c => c.EmergencyContact);
    }

    [Fact]
    public void Should_pass_when_emergency_contact_is_null()
    {
        var result = _validator.TestValidate(Valid(emergency: null));
        result.ShouldNotHaveValidationErrorFor(c => c.EmergencyContact);
    }
}
