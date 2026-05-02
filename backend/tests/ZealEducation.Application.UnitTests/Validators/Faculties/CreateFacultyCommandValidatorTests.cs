using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Faculties.Commands.CreateFaculty;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.UnitTests.Validators.Faculties;

public class CreateFacultyCommandValidatorTests
{
    private readonly CreateFacultyCommandValidator _validator = new();

    private static CreateFacultyCommand Valid(
        string username = "john.doe",
        string password = "Password1",
        string fullName = "John Doe",
        string email = "john@example.com",
        string phone = "0123456789",
        DateOnly? dob = null,
        Gender gender = Gender.Male,
        string position = "Lecturer",
        string department = "Computer Science",
        DateOnly? joinedDate = null,
        string facultyCode = "F-001",
        string qualification = "PhD",
        string specialization = "AI",
        int experienceYears = 5) => new(
            username,
            password,
            fullName,
            email,
            phone,
            dob ?? new DateOnly(1990, 1, 1),
            gender,
            position,
            department,
            joinedDate,
            facultyCode,
            qualification,
            specialization,
            experienceYears);

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_username_is_empty(string username)
    {
        var result = _validator.TestValidate(Valid(username: username));
        result.ShouldHaveValidationErrorFor(c => c.Username);
    }

    [Fact]
    public void Should_fail_when_username_is_below_minimum_length()
    {
        var result = _validator.TestValidate(Valid(username: "ab"));
        result.ShouldHaveValidationErrorFor(c => c.Username);
    }

    [Fact]
    public void Should_fail_when_username_exceeds_50_chars()
    {
        var result = _validator.TestValidate(Valid(username: new string('a', 51)));
        result.ShouldHaveValidationErrorFor(c => c.Username);
    }

    [Theory]
    [InlineData("john doe")]
    [InlineData("john-doe")]
    [InlineData("john@doe")]
    public void Should_fail_when_username_has_invalid_chars(string username)
    {
        var result = _validator.TestValidate(Valid(username: username));
        result.ShouldHaveValidationErrorFor(c => c.Username);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_password_is_empty(string password)
    {
        var result = _validator.TestValidate(Valid(password: password));
        result.ShouldHaveValidationErrorFor(c => c.Password);
    }

    [Fact]
    public void Should_fail_when_password_is_below_minimum_length()
    {
        var result = _validator.TestValidate(Valid(password: "Pass1"));
        result.ShouldHaveValidationErrorFor(c => c.Password);
    }

    [Fact]
    public void Should_fail_when_password_exceeds_100_chars()
    {
        var result = _validator.TestValidate(Valid(password: new string('p', 101)));
        result.ShouldHaveValidationErrorFor(c => c.Password);
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
        var result = _validator.TestValidate(Valid(fullName: new string('n', 101)));
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
    public void Should_fail_when_email_is_invalid_format()
    {
        var result = _validator.TestValidate(Valid(email: "not-an-email"));
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Should_fail_when_email_exceeds_100_chars()
    {
        var longEmail = new string('a', 96) + "@b.co";
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
    public void Should_fail_when_phone_is_not_10_digits(string phone)
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
    public void Should_fail_when_gender_is_invalid_enum()
    {
        var result = _validator.TestValidate(Valid(gender: (Gender)99));
        result.ShouldHaveValidationErrorFor(c => c.Gender);
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
    public void Should_pass_when_joined_date_is_null()
    {
        var result = _validator.TestValidate(Valid(joinedDate: null));
        result.ShouldNotHaveValidationErrorFor(c => c.JoinedDate);
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

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_faculty_code_is_empty(string code)
    {
        var result = _validator.TestValidate(Valid(facultyCode: code));
        result.ShouldHaveValidationErrorFor(c => c.FacultyCode);
    }

    [Fact]
    public void Should_fail_when_faculty_code_exceeds_20_chars()
    {
        var result = _validator.TestValidate(Valid(facultyCode: new string('F', 21)));
        result.ShouldHaveValidationErrorFor(c => c.FacultyCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_qualification_is_empty(string qualification)
    {
        var result = _validator.TestValidate(Valid(qualification: qualification));
        result.ShouldHaveValidationErrorFor(c => c.Qualification);
    }

    [Fact]
    public void Should_fail_when_qualification_exceeds_100_chars()
    {
        var result = _validator.TestValidate(Valid(qualification: new string('q', 101)));
        result.ShouldHaveValidationErrorFor(c => c.Qualification);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_specialization_is_empty(string specialization)
    {
        var result = _validator.TestValidate(Valid(specialization: specialization));
        result.ShouldHaveValidationErrorFor(c => c.Specialization);
    }

    [Fact]
    public void Should_fail_when_specialization_exceeds_100_chars()
    {
        var result = _validator.TestValidate(Valid(specialization: new string('s', 101)));
        result.ShouldHaveValidationErrorFor(c => c.Specialization);
    }

    [Fact]
    public void Should_fail_when_experience_years_is_negative()
    {
        var result = _validator.TestValidate(Valid(experienceYears: -1));
        result.ShouldHaveValidationErrorFor(c => c.ExperienceYears);
    }

    [Fact]
    public void Should_fail_when_experience_years_is_above_80()
    {
        var result = _validator.TestValidate(Valid(experienceYears: 81));
        result.ShouldHaveValidationErrorFor(c => c.ExperienceYears);
    }
}
