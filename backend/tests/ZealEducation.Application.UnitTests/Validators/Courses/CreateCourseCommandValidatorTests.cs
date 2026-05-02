using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Courses.Commands.CreateCourse;

namespace ZealEducation.Application.UnitTests.Validators.Courses;

public class CreateCourseCommandValidatorTests
{
    private readonly CreateCourseCommandValidator _validator = new();

    private static CreateCourseCommand Valid(
        string? courseName = "Intro to Math",
        string? description = "A short description",
        int durationWeeks = 12,
        decimal baseFee = 100m,
        bool isActive = true) => new(
            courseName!,
            description,
            durationWeeks,
            baseFee,
            isActive);

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_course_name_is_empty(string name)
    {
        var result = _validator.TestValidate(Valid(courseName: name));
        result.ShouldHaveValidationErrorFor(c => c.CourseName);
    }

    [Fact]
    public void Should_fail_when_course_name_exceeds_100_chars()
    {
        var result = _validator.TestValidate(Valid(courseName: new string('C', 101)));
        result.ShouldHaveValidationErrorFor(c => c.CourseName);
    }

    [Fact]
    public void Should_pass_when_course_name_is_exactly_100_chars()
    {
        var result = _validator.TestValidate(Valid(courseName: new string('C', 100)));
        result.ShouldNotHaveValidationErrorFor(c => c.CourseName);
    }

    [Fact]
    public void Should_fail_when_description_exceeds_4000_chars()
    {
        var result = _validator.TestValidate(Valid(description: new string('d', 4001)));
        result.ShouldHaveValidationErrorFor(c => c.Description);
    }

    [Fact]
    public void Should_pass_when_description_is_null()
    {
        var result = _validator.TestValidate(Valid(description: null));
        result.ShouldNotHaveValidationErrorFor(c => c.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_fail_when_duration_weeks_is_not_positive(int weeks)
    {
        var result = _validator.TestValidate(Valid(durationWeeks: weeks));
        result.ShouldHaveValidationErrorFor(c => c.DurationWeeks);
    }

    [Fact]
    public void Should_fail_when_duration_weeks_exceeds_520()
    {
        var result = _validator.TestValidate(Valid(durationWeeks: 521));
        result.ShouldHaveValidationErrorFor(c => c.DurationWeeks);
    }

    [Fact]
    public void Should_pass_when_duration_weeks_is_exactly_520()
    {
        var result = _validator.TestValidate(Valid(durationWeeks: 520));
        result.ShouldNotHaveValidationErrorFor(c => c.DurationWeeks);
    }

    [Fact]
    public void Should_fail_when_base_fee_is_negative()
    {
        var result = _validator.TestValidate(Valid(baseFee: -0.01m));
        result.ShouldHaveValidationErrorFor(c => c.BaseFee);
    }

    [Fact]
    public void Should_pass_when_base_fee_is_zero()
    {
        var result = _validator.TestValidate(Valid(baseFee: 0m));
        result.ShouldNotHaveValidationErrorFor(c => c.BaseFee);
    }
}
