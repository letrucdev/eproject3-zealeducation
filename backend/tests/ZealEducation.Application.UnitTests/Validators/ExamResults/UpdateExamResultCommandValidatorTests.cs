using FluentValidation.TestHelper;
using ZealEducation.Application.Features.ExamResults.Commands.UpdateExamResult;

namespace ZealEducation.Application.UnitTests.Validators.ExamResults;

public class UpdateExamResultCommandValidatorTests
{
    private readonly UpdateExamResultCommandValidator _validator = new();

    private static UpdateExamResultCommand Valid(
        Guid? resultId = null,
        decimal score = 75m,
        bool isFinalized = false) => new(
            resultId ?? Guid.NewGuid(),
            score,
            isFinalized);

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

    [Fact]
    public void Should_pass_when_is_finalized_is_true()
    {
        var result = _validator.TestValidate(Valid(isFinalized: true));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
