using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Examinations.Queries.GetBatchExamScoresTrend;

namespace ZealEducation.Application.UnitTests.Validators.Examinations;

public class GetBatchExamScoresTrendQueryValidatorTests
{
    private readonly GetBatchExamScoresTrendQueryValidator _validator = new();

    private static GetBatchExamScoresTrendQuery Valid(Guid? batchId = null, int days = 7) =>
        new(batchId ?? Guid.NewGuid(), days);

    [Theory]
    [InlineData(7)]
    [InlineData(30)]
    [InlineData(90)]
    public void Should_pass_when_batch_id_and_days_are_valid(int days)
    {
        var result = _validator.TestValidate(Valid(days: days));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_batch_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(batchId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.BatchId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(29)]
    [InlineData(31)]
    [InlineData(89)]
    [InlineData(91)]
    [InlineData(180)]
    [InlineData(-1)]
    public void Should_fail_when_days_is_not_an_allowed_value(int days)
    {
        var result = _validator.TestValidate(Valid(days: days));
        result.ShouldHaveValidationErrorFor(c => c.Days);
    }

    [Fact]
    public void Should_fail_when_both_batch_id_is_empty_and_days_is_invalid()
    {
        var result = _validator.TestValidate(Valid(batchId: Guid.Empty, days: 5));
        result.ShouldHaveValidationErrorFor(c => c.BatchId);
        result.ShouldHaveValidationErrorFor(c => c.Days);
    }
}
