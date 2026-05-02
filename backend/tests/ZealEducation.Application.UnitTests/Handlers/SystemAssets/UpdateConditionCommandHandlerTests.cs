using ZealEducation.Application.Features.SystemAssets.Commands.UpdateCondition;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Exceptions;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.SystemAssets;

public class UpdateConditionCommandHandlerTests
{
    private readonly Mock<IRepository<SystemAsset>> _assetRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private UpdateConditionCommandHandler CreateHandler() =>
        new(_assetRepo.Object, _uow.Object);

    private static SystemAsset BuildAsset(Guid id)
    {
        var asset = new SystemAsset(
            "Projector",
            "Electronics",
            "SN-OG-1",
            "Room A",
            new DateTime(2023, 1, 1),
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
            new UpdateConditionCommand(assetId, ConditionStatus.Maintenance), default);

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
            new UpdateConditionCommand(assetId, ConditionStatus.Maintenance), default);

        await act.Should().ThrowAsync<SystemAssetDecommissionedException>()
            .WithMessage("Cannot update a decommissioned asset.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Updates_condition_status()
    {
        var assetId = Guid.NewGuid();
        var asset = BuildAsset(assetId);
        _assetRepo.SetupGetById(assetId, asset);

        SystemAsset? updated = null;
        _assetRepo.Setup(r => r.Update(It.IsAny<SystemAsset>()))
            .Callback<SystemAsset>(a => updated = a);

        await CreateHandler().Handle(
            new UpdateConditionCommand(assetId, ConditionStatus.Faulty), default);

        updated.Should().NotBeNull();
        updated!.ConditionStatus.Should().Be(ConditionStatus.Faulty);
        updated.LastMaintenance.Should().BeNull();

        _assetRepo.Verify(r => r.Update(It.Is<SystemAsset>(a => a.Id == assetId)), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Sets_last_maintenance_when_status_is_maintenance()
    {
        var assetId = Guid.NewGuid();
        var asset = BuildAsset(assetId);
        _assetRepo.SetupGetById(assetId, asset);

        var before = DateTime.UtcNow.AddSeconds(-1);

        await CreateHandler().Handle(
            new UpdateConditionCommand(assetId, ConditionStatus.Maintenance), default);

        asset.ConditionStatus.Should().Be(ConditionStatus.Maintenance);
        asset.LastMaintenance.Should().NotBeNull();
        asset.LastMaintenance!.Value.Should().BeOnOrAfter(before);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
