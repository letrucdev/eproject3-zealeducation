using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Candidates.Commands.UpdateCandidate;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.UnitTests.Validators.Candidates;

public class UpdateCandidateCommandValidatorTests
{
    private readonly UpdateCandidateCommandValidator _validator = new();

    private static UpdateCandidateCommand Valid(
        Guid? candidateId = null,
        string fullName = "John Doe",
        string email = "john@example.com",
        string phone = "0900000000",
        string? address = "1 Main St",
        string? emergencyContact = "Jane Doe",
        string? notes = null,
        CandidateStatus status = CandidateStatus.Active) => new(
            candidateId ?? Guid.NewGuid(),
            fullName,
            email,
            phone,
            address,
            emergencyContact,
            notes,
            status);

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_candidate_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(candidateId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.CandidateId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_full_name_is_empty(string fullName)
    {
        var result = _validator.TestValidate(Valid(fullName: fullName));
        result.ShouldHaveValidationErrorFor(c => c.FullName);
    }

    [Fact]
    public void Should_fail_when_full_name_exceeds_100_chars()
    {
        var result = _validator.TestValidate(Valid(fullName: new string('a', 101)));
        result.ShouldHaveValidationErrorFor(c => c.FullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_email_is_empty(string email)
    {
        var result = _validator.TestValidate(Valid(email: email));
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("plainaddress")]
    public void Should_fail_when_email_is_not_valid(string email)
    {
        var result = _validator.TestValidate(Valid(email: email));
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Should_fail_when_email_exceeds_100_chars()
    {
        var localPart = new string('a', 95);
        var result = _validator.TestValidate(Valid(email: $"{localPart}@example.com"));
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_phone_is_empty(string phone)
    {
        var result = _validator.TestValidate(Valid(phone: phone));
        result.ShouldHaveValidationErrorFor(c => c.Phone);
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    public void Should_fail_when_phone_is_not_10_digits(string phone)
    {
        var result = _validator.TestValidate(Valid(phone: phone));
        result.ShouldHaveValidationErrorFor(c => c.Phone);
    }

    [Theory]
    [InlineData("090000000a")]
    [InlineData("09000-0000")]
    [InlineData("abcdefghij")]
    public void Should_fail_when_phone_contains_non_digits(string phone)
    {
        var result = _validator.TestValidate(Valid(phone: phone));
        result.ShouldHaveValidationErrorFor(c => c.Phone);
    }

    [Fact]
    public void Should_fail_when_emergency_contact_exceeds_100_chars()
    {
        var result = _validator.TestValidate(Valid(emergencyContact: new string('z', 101)));
        result.ShouldHaveValidationErrorFor(c => c.EmergencyContact);
    }

    [Fact]
    public void Should_pass_when_emergency_contact_is_null()
    {
        var result = _validator.TestValidate(Valid(emergencyContact: null));
        result.ShouldNotHaveValidationErrorFor(c => c.EmergencyContact);
    }

    [Fact]
    public void Should_fail_when_status_is_not_in_enum()
    {
        var result = _validator.TestValidate(Valid(status: (CandidateStatus)999));
        result.ShouldHaveValidationErrorFor(c => c.Status);
    }
}
