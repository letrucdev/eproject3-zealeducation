using FluentValidation.TestHelper;
using ZealEducation.Application.Features.SystemAssets.Commands.CreateSystemAsset;

namespace ZealEducation.Application.UnitTests.Validators.SystemAssets;

public class CreateSystemAssetCommandValidatorTests
{
    private readonly CreateSystemAssetCommandValidator _validator = new();

    private static CreateSystemAssetCommand Valid(
        string assetName = "Projector",
        string assetType = "Electronics",
        string serialNumber = "SN-001",
        string location = "Room A",
        DateOnly? purchaseDate = null,
        string? notes = null) => new(
            assetName,
            assetType,
            serialNumber,
            location,
            purchaseDate ?? DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1),
            notes);

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
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
    public void Should_fail_when_serial_number_is_empty(string serial)
    {
        var result = _validator.TestValidate(Valid(serialNumber: serial));
        result.ShouldHaveValidationErrorFor(c => c.SerialNumber);
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

    [Fact]
    public void Should_fail_when_purchase_date_is_in_the_future()
    {
        var future = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var result = _validator.TestValidate(Valid(purchaseDate: future));
        result.ShouldHaveValidationErrorFor(c => c.PurchaseDate);
    }

    [Fact]
    public void Should_pass_when_purchase_date_is_in_the_past()
    {
        var result = _validator.TestValidate(Valid(purchaseDate: DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1)));
        result.ShouldNotHaveValidationErrorFor(c => c.PurchaseDate);
    }
}
