using FluentValidation.TestHelper;
using ZealEducation.Application.Features.SystemAssets.Commands.UpdateCondition;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.UnitTests.Validators.SystemAssets;

public class UpdateConditionCommandValidatorTests
{
    private readonly UpdateConditionCommandValidator _validator = new();

    private static UpdateConditionCommand Valid(
        Guid? id = null,
        ConditionStatus status = ConditionStatus.Good) => new(
            id ?? Guid.NewGuid(),
            status);

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(id: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Should_fail_when_condition_status_is_not_a_defined_enum_value()
    {
        var result = _validator.TestValidate(Valid(status: (ConditionStatus)999));
        result.ShouldHaveValidationErrorFor(c => c.ConditionStatus);
    }

    [Theory]
    [InlineData(ConditionStatus.Good)]
    [InlineData(ConditionStatus.Maintenance)]
    [InlineData(ConditionStatus.Faulty)]
    [InlineData(ConditionStatus.Decommissioned)]
    public void Should_pass_for_each_defined_condition_status(ConditionStatus status)
    {
        var result = _validator.TestValidate(Valid(status: status));
        result.ShouldNotHaveValidationErrorFor(c => c.ConditionStatus);
    }
}
