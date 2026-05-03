using FluentValidation.TestHelper;
using ZealEducation.Application.Features.SystemAssets.Commands.UpdateSystemAsset;

namespace ZealEducation.Application.UnitTests.Validators.SystemAssets;

public class UpdateSystemAssetCommandValidatorTests
{
    private readonly UpdateSystemAssetCommandValidator _validator = new();

    private static UpdateSystemAssetCommand Valid(
        Guid? id = null,
        string assetName = "Projector",
        string assetType = "Electronics",
        string location = "Room A",
        string? notes = null) => new(
            id ?? Guid.NewGuid(),
            assetName,
            assetType,
            location,
            notes);

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

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_asset_name_is_empty(string name)
    {
        var result = _validator.TestValidate(Valid(assetName: name));
        result.ShouldHaveValidationErrorFor(c => c.AssetName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_asset_type_is_empty(string type)
    {
        var result = _validator.TestValidate(Valid(assetType: type));
        result.ShouldHaveValidationErrorFor(c => c.AssetType);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_location_is_empty(string location)
    {
        var result = _validator.TestValidate(Valid(location: location));
        result.ShouldHaveValidationErrorFor(c => c.Location);
    }
}
