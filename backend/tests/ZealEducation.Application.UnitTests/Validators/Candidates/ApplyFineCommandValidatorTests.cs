using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Candidates.Commands.ApplyFine;

namespace ZealEducation.Application.UnitTests.Validators.Candidates;

public class ApplyFineCommandValidatorTests
{
    private readonly ApplyFineCommandValidator _validator = new();

    private static ApplyFineCommand Valid(
        Guid? candidateId = null,
        string violationReason = "Late submission of assignment",
        decimal penaltyAmount = 100m) => new(
            candidateId ?? Guid.NewGuid(),
            violationReason,
            penaltyAmount);

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
    [InlineData("   ")]
    public void Should_fail_when_violation_reason_is_empty_or_whitespace(string reason)
    {
        var result = _validator.TestValidate(Valid(violationReason: reason));
        result.ShouldHaveValidationErrorFor(c => c.ViolationReason);
    }

    [Fact]
    public void Should_fail_when_violation_reason_exceeds_500_chars()
    {
        var result = _validator.TestValidate(Valid(violationReason: new string('x', 501)));
        result.ShouldHaveValidationErrorFor(c => c.ViolationReason);
    }

    [Fact]
    public void Should_pass_when_violation_reason_is_at_max_length()
    {
        var result = _validator.TestValidate(Valid(violationReason: new string('x', 500)));
        result.ShouldNotHaveValidationErrorFor(c => c.ViolationReason);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Should_fail_when_penalty_amount_is_not_greater_than_zero(decimal amount)
    {
        var result = _validator.TestValidate(Valid(penaltyAmount: amount));
        result.ShouldHaveValidationErrorFor(c => c.PenaltyAmount);
    }

    [Fact]
    public void Should_pass_when_penalty_amount_is_positive()
    {
        var result = _validator.TestValidate(Valid(penaltyAmount: 0.01m));
        result.ShouldNotHaveValidationErrorFor(c => c.PenaltyAmount);
    }
}
