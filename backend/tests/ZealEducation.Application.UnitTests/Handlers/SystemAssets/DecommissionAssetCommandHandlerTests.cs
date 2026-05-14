using ZealEducation.Application.Features.SystemAssets.Commands.DecommissionAsset;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Exceptions;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.SystemAssets;

public class DecommissionAssetCommandHandlerTests
{
    private readonly Mock<IRepository<SystemAsset>> _assetRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private DecommissionAssetCommandHandler CreateHandler() =>
        new(_assetRepo.Object, _uow.Object);

    private static SystemAsset BuildAsset(Guid id)
    {
        var asset = new SystemAsset(
            "Projector",
            "Electronics",
            "SN-OG-1",
            "Room A",
            new DateOnly(2023, 1, 1),
            Guid.NewGuid());
        asset.Id = id;
        return asset;
    }

    [Fact]
    public async Task Throws_when_asset_does_not_exist()
    {
        var assetId = Guid.NewGuid();
        _assetRepo.SetupGetById(assetId, null);

        var act = async () => await CreateHandler().Handle(
            new DecommissionAssetCommand(assetId), default);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .Where(e => e.Message.Contains(assetId.ToString()));
        _assetRepo.Verify(r => r.Update(It.IsAny<SystemAsset>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_when_asset_is_already_decommissioned()
    {
        var assetId = Guid.NewGuid();
        var asset = BuildAsset(assetId);
        asset.Decommission();
        _assetRepo.SetupGetById(assetId, asset);

        var act = async () => await CreateHandler().Handle(
            new DecommissionAssetCommand(assetId), default);

        await act.Should().ThrowAsync<SystemAssetDecommissionedException>()
            .WithMessage("Asset is already decommissioned.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Decommissions_asset_and_persists_change()
    {
        var assetId = Guid.NewGuid();
        var asset = BuildAsset(assetId);
        _assetRepo.SetupGetById(assetId, asset);

        SystemAsset? updated = null;
        _assetRepo.Setup(r => r.Update(It.IsAny<SystemAsset>()))
            .Callback<SystemAsset>(a => updated = a);

        await CreateHandler().Handle(new DecommissionAssetCommand(assetId), default);

        updated.Should().NotBeNull();
        updated!.Id.Should().Be(assetId);
        updated.ConditionStatus.Should().Be(ConditionStatus.Decommissioned);

        _assetRepo.Verify(r => r.Update(It.Is<SystemAsset>(a => a.Id == assetId)), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
