using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Examinations.Queries.GetBatchGradeDistribution;

namespace ZealEducation.Application.UnitTests.Validators.Examinations;

public class GetBatchGradeDistributionQueryValidatorTests
{
    private readonly GetBatchGradeDistributionQueryValidator _validator = new();

    private static GetBatchGradeDistributionQuery Valid(Guid? batchId = null) =>
        new(batchId ?? Guid.NewGuid());

    [Fact]
    public void Should_pass_when_batch_id_is_provided()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_batch_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(batchId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.BatchId);
    }
}
