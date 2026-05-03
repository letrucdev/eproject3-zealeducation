using FluentValidation.TestHelper;
using ZealEducation.Application.Features.ExamResults.Commands.OverrideExamResult;

namespace ZealEducation.Application.UnitTests.Validators.ExamResults;

public class OverrideExamResultCommandValidatorTests
{
    private readonly OverrideExamResultCommandValidator _validator = new();

    private static OverrideExamResultCommand Valid(
        Guid? resultId = null,
        decimal score = 80m,
        string? overrideReason = "Manual review of paper") => new(
            resultId ?? Guid.NewGuid(),
            score,
            overrideReason!);

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_result_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(resultId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.ResultId);
    }

    [Fact]
    public void Should_fail_when_score_is_negative()
    {
        var result = _validator.TestValidate(Valid(score: -1m));
        result.ShouldHaveValidationErrorFor(c => c.Score);
    }

    [Fact]
    public void Should_pass_when_score_is_zero()
    {
        var result = _validator.TestValidate(Valid(score: 0m));
        result.ShouldNotHaveValidationErrorFor(c => c.Score);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_override_reason_is_empty_or_whitespace(string reason)
    {
        var result = _validator.TestValidate(Valid(overrideReason: reason));
        result.ShouldHaveValidationErrorFor(c => c.OverrideReason)
            .WithErrorMessage("Override reason is required.");
    }

    [Fact]
    public void Should_fail_when_override_reason_is_too_short()
    {
        var result = _validator.TestValidate(Valid(overrideReason: "abcd"));
        result.ShouldHaveValidationErrorFor(c => c.OverrideReason)
            .WithErrorMessage("Override reason must be at least 5 characters.");
    }

    [Fact]
    public void Should_pass_when_override_reason_is_exactly_5_chars()
    {
        var result = _validator.TestValidate(Valid(overrideReason: "abcde"));
        result.ShouldNotHaveValidationErrorFor(c => c.OverrideReason);
    }

    [Fact]
    public void Should_fail_when_override_reason_exceeds_500_chars()
    {
        var result = _validator.TestValidate(Valid(overrideReason: new string('x', 501)));
        result.ShouldHaveValidationErrorFor(c => c.OverrideReason);
    }

    [Fact]
    public void Should_pass_when_override_reason_is_exactly_500_chars()
    {
        var result = _validator.TestValidate(Valid(overrideReason: new string('x', 500)));
        result.ShouldNotHaveValidationErrorFor(c => c.OverrideReason);
    }
}
