using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.SystemAssets.Commands.CreateSystemAsset;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.SystemAssets;

public class CreateSystemAssetCommandHandlerTests
{
    private readonly Mock<IRepository<SystemAsset>> _assetRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private CreateSystemAssetCommandHandler CreateHandler() =>
        new(_assetRepo.Object, _staffRepo.Object, _uow.Object, _currentUser.Object);

    private static CreateSystemAssetCommand Cmd(
        string assetName = "Projector",
        string assetType = "Electronics",
        string serialNumber = "SN-001",
        string location = "Room A",
        DateTime? purchaseDate = null,
        string? notes = null) => new(
            assetName,
            assetType,
            serialNumber,
            location,
            purchaseDate ?? new DateTime(2024, 1, 1),
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

    [Fact]
    public async Task Throws_unauthorized_when_logged_in_account_is_not_linked_to_a_staff()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _staffRepo.SetupFind(Array.Empty<Staff>());

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Logged-in account is not linked to a Staff.");
        _assetRepo.Verify(r => r.AddAsync(It.IsAny<SystemAsset>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_when_serial_number_already_exists()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);

        _staffRepo.Setup(r => r.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Staff, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Staff> { ExistingStaff(userId) });

        _assetRepo.Setup(r => r.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<SystemAsset, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SystemAsset>
            {
                new SystemAsset("Existing", "Electronics", "SN-001", "Room B", new DateTime(2023, 1, 1), Guid.NewGuid())
            });

        var act = async () => await CreateHandler().Handle(Cmd(serialNumber: "SN-001"), default);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("SerialNumber 'SN-001' already exists.");
        _assetRepo.Verify(r => r.AddAsync(It.IsAny<SystemAsset>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Creates_asset_when_command_is_valid()
    {
        var userId = Guid.NewGuid();
        var staff = ExistingStaff(userId);
        _currentUser.Setup(c => c.UserId).Returns(userId);

        _staffRepo.Setup(r => r.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Staff, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Staff> { staff });

        _assetRepo.Setup(r => r.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<SystemAsset, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SystemAsset>());

        SystemAsset? captured = null;
        _assetRepo.Setup(r => r.AddAsync(It.IsAny<SystemAsset>(), It.IsAny<CancellationToken>()))
            .Callback<SystemAsset, CancellationToken>((a, _) => captured = a)
            .ReturnsAsync((SystemAsset a, CancellationToken _) => a);

        var purchaseDate = new DateTime(2024, 2, 10);
        var resultId = await CreateHandler().Handle(
            Cmd(assetName: "Laptop", assetType: "Electronics", serialNumber: "SN-LP-9",
                location: "Lab 1", purchaseDate: purchaseDate, notes: "Spare unit"),
            default);

        captured.Should().NotBeNull();
        resultId.Should().Be(captured!.Id);
        captured.AssetName.Should().Be("Laptop");
        captured.AssetType.Should().Be("Electronics");
        captured.SerialNumber.Should().Be("SN-LP-9");
        captured.Location.Should().Be("Lab 1");
        captured.PurchaseDate.Should().Be(purchaseDate);
        captured.ManagedBy.Should().Be(staff.Id);
        captured.Notes.Should().Be("Spare unit");
        captured.ConditionStatus.Should().Be(ConditionStatus.Good);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
