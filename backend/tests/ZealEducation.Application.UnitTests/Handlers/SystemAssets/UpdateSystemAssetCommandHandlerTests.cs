using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.SystemAssets.Commands.UpdateSystemAsset;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Exceptions;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.SystemAssets;

public class UpdateSystemAssetCommandHandlerTests
{
    private readonly Mock<IRepository<SystemAsset>> _assetRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private UpdateSystemAssetCommandHandler CreateHandler() =>
        new(_assetRepo.Object, _staffRepo.Object, _uow.Object, _currentUser.Object);

    private static UpdateSystemAssetCommand Cmd(
        Guid? id = null,
        string assetName = "Updated Name",
        string assetType = "Electronics",
        string location = "Room B",
        string? notes = null) => new(
            id ?? Guid.NewGuid(),
            assetName,
            assetType,
            location,
            notes);

    private static Staff ExistingStaff(Guid userAccountId) => new()
    {
        Id = Guid.NewGuid(),
        UserAccountId = userAccountId,
        Position = "IT",
        Department = "Operations",
        JoinedDate = new DateOnly(2020, 1, 1),
        IsActive = true
    };

    private static SystemAsset ExistingAsset(Guid? id = null, Guid? managedBy = null) =>
        BuildAsset(id ?? Guid.NewGuid(), managedBy ?? Guid.NewGuid());

    private static SystemAsset BuildAsset(Guid id, Guid managedBy)
    {
        var asset = new SystemAsset(
            "Original Name",
            "Original Type",
            "SN-OG-1",
            "Room A",
            new DateOnly(2023, 1, 1),
            managedBy,
            "Original notes");
        asset.Id = id;
        return asset;
    }

    [Fact]
    public async Task Throws_unauthorized_when_logged_in_account_is_not_linked_to_a_staff()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _staffRepo.SetupFind(Array.Empty<Staff>());

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Logged-in account is not linked to a Staff.");
        _assetRepo.Verify(r => r.Update(It.IsAny<SystemAsset>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_when_asset_does_not_exist()
    {
        var userId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _staffRepo.SetupFind(new[] { ExistingStaff(userId) });
        _assetRepo.SetupGetById(assetId, null);

        var act = async () => await CreateHandler().Handle(Cmd(id: assetId), default);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .Where(e => e.Message.Contains(assetId.ToString()));
        _assetRepo.Verify(r => r.Update(It.IsAny<SystemAsset>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_when_asset_is_already_decommissioned()
    {
        var userId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _staffRepo.SetupFind(new[] { ExistingStaff(userId) });

        var asset = ExistingAsset(assetId);
        asset.Decommission();
        _assetRepo.SetupGetById(assetId, asset);

        var act = async () => await CreateHandler().Handle(Cmd(id: assetId), default);

        await act.Should().ThrowAsync<SystemAssetDecommissionedException>()
            .WithMessage("Cannot update a decommissioned asset.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Updates_asset_when_command_is_valid()
    {
        var userId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var staff = ExistingStaff(userId);
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _staffRepo.SetupFind(new[] { staff });

        var asset = ExistingAsset(assetId);
        _assetRepo.SetupGetById(assetId, asset);

        SystemAsset? updated = null;
        _assetRepo.Setup(r => r.Update(It.IsAny<SystemAsset>()))
            .Callback<SystemAsset>(a => updated = a);

        await CreateHandler().Handle(
            Cmd(id: assetId, assetName: "New Name", assetType: "New Type", location: "New Location", notes: "New Notes"),
            default);

        updated.Should().NotBeNull();
        updated!.Id.Should().Be(assetId);
        updated.AssetName.Should().Be("New Name");
        updated.AssetType.Should().Be("New Type");
        updated.Location.Should().Be("New Location");
        updated.Notes.Should().Be("New Notes");
        updated.ManagedBy.Should().Be(staff.Id);

        _assetRepo.Verify(r => r.Update(It.Is<SystemAsset>(a => a.Id == assetId)), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
