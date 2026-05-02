using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Staffs.Commands.UpdateStaff;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.UnitTests.Validators.Staffs;

public class UpdateStaffCommandValidatorTests
{
    private readonly UpdateStaffCommandValidator _validator = new();

    private static UpdateStaffCommand Valid(
        Guid? staffId = null,
        string fullName = "Alice Doe",
        string email = "alice@example.com",
        string phone = "0123456789",
        DateOnly? dob = null,
        Gender gender = Gender.Female,
        UserRole role = UserRole.Counselor,
        string position = "Counselor",
        string department = "Admissions",
        DateOnly? joinedDate = null,
        bool isActive = true) => new(
            staffId ?? Guid.NewGuid(),
            fullName,
            email,
            phone,
            dob ?? new DateOnly(1995, 1, 1),
            gender,
            role,
            position,
            department,
            joinedDate ?? new DateOnly(2020, 1, 1),
            isActive);

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_staff_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(staffId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.StaffId);
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

    [Fact]
    public void Should_fail_when_email_is_invalid()
    {
        var result = _validator.TestValidate(Valid(email: "not-an-email"));
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Should_fail_when_email_exceeds_100_chars()
    {
        var longEmail = new string('a', 96) + "@b.io";
        var result = _validator.TestValidate(Valid(email: longEmail));
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
    public void Should_fail_when_dob_is_today_or_future()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = _validator.TestValidate(Valid(dob: today));
        result.ShouldHaveValidationErrorFor(c => c.Dob);
    }

    [Fact]
    public void Should_fail_when_gender_is_out_of_enum()
    {
        var result = _validator.TestValidate(Valid(gender: (Gender)99));
        result.ShouldHaveValidationErrorFor(c => c.Gender);
    }

    [Fact]
    public void Should_fail_when_role_is_out_of_enum()
    {
        var result = _validator.TestValidate(Valid(role: (UserRole)99));
        result.ShouldHaveValidationErrorFor(c => c.Role);
    }

    [Theory]
    [InlineData(UserRole.SystemAdmin)]
    [InlineData(UserRole.Faculty)]
    [InlineData(UserRole.Candidate)]
    public void Should_fail_when_role_is_not_an_allowed_staff_role(UserRole role)
    {
        var result = _validator.TestValidate(Valid(role: role));
        result.ShouldHaveValidationErrorFor(c => c.Role);
    }

    [Theory]
    [InlineData(UserRole.Incharge)]
    [InlineData(UserRole.Counselor)]
    [InlineData(UserRole.AccountsStaff)]
    public void Should_pass_for_each_allowed_staff_role(UserRole role)
    {
        var result = _validator.TestValidate(Valid(role: role));
        result.ShouldNotHaveValidationErrorFor(c => c.Role);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_position_is_empty(string position)
    {
        var result = _validator.TestValidate(Valid(position: position));
        result.ShouldHaveValidationErrorFor(c => c.Position);
    }

    [Fact]
    public void Should_fail_when_position_exceeds_60_chars()
    {
        var result = _validator.TestValidate(Valid(position: new string('p', 61)));
        result.ShouldHaveValidationErrorFor(c => c.Position);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_department_is_empty(string department)
    {
        var result = _validator.TestValidate(Valid(department: department));
        result.ShouldHaveValidationErrorFor(c => c.Department);
    }

    [Fact]
    public void Should_fail_when_department_exceeds_60_chars()
    {
        var result = _validator.TestValidate(Valid(department: new string('d', 61)));
        result.ShouldHaveValidationErrorFor(c => c.Department);
    }

    [Fact]
    public void Should_pass_when_joined_date_is_today()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = _validator.TestValidate(Valid(joinedDate: today));
        result.ShouldNotHaveValidationErrorFor(c => c.JoinedDate);
    }

    [Fact]
    public void Should_fail_when_joined_date_is_in_the_future()
    {
        var future = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var result = _validator.TestValidate(Valid(joinedDate: future));
        result.ShouldHaveValidationErrorFor(c => c.JoinedDate);
    }
}
