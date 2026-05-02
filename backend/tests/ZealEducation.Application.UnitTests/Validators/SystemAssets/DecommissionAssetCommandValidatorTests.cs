using FluentValidation.TestHelper;
using ZealEducation.Application.Features.SystemAssets.Commands.DecommissionAsset;

namespace ZealEducation.Application.UnitTests.Validators.SystemAssets;

public class DecommissionAssetCommandValidatorTests
{
    private readonly DecommissionAssetCommandValidator _validator = new();

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(new DecommissionAssetCommand(Guid.NewGuid()));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_id_is_empty()
    {
        var result = _validator.TestValidate(new DecommissionAssetCommand(Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }
}
