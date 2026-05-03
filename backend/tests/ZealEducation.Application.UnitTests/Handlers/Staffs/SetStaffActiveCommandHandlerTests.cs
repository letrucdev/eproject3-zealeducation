using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Staffs.Commands.SetStaffActive;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Staffs;

public class SetStaffActiveCommandHandlerTests
{
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private SetStaffActiveCommandHandler CreateHandler() =>
        new(_staffRepo.Object, _userRepo.Object, _uow.Object);

    private static Staff BuildStaff(Guid id, Guid userAccountId, bool isActive) => new()
    {
        Id = id,
        UserAccountId = userAccountId,
        Position = "Counselor",
        Department = "Admissions",
        JoinedDate = new DateOnly(2020, 1, 1),
        IsActive = isActive
    };

    private static UserAccount BuildUser(Guid id, bool isActive) => new()
    {
        Id = id,
        Username = "alice.doe",
        PasswordHash = "$hash$",
        FullName = "Alice Doe",
        Email = "alice@example.com",
        Phone = "0123456789",
        Dob = new DateOnly(1995, 1, 1),
        Gender = Gender.Female,
        Role = UserRole.Counselor,
        IsActive = isActive
    };

    [Fact]
    public async Task Throws_NotFoundException_when_staff_does_not_exist()
    {
        var staffId = Guid.NewGuid();
        _staffRepo.SetupGetById(staffId, null);

        var act = async () => await CreateHandler().Handle(new SetStaffActiveCommand(staffId, true), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _staffRepo.Verify(r => r.Update(It.IsAny<Staff>()), Times.Never);
        _userRepo.Verify(r => r.Update(It.IsAny<UserAccount>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_user_account_does_not_exist()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _staffRepo.SetupGetById(staffId, BuildStaff(staffId, userId, isActive: true));
        _userRepo.SetupGetById(userId, null);

        var act = async () => await CreateHandler().Handle(new SetStaffActiveCommand(staffId, false), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _staffRepo.Verify(r => r.Update(It.IsAny<Staff>()), Times.Never);
        _userRepo.Verify(r => r.Update(It.IsAny<UserAccount>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Deactivates_staff_and_user_when_setting_inactive()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var staff = BuildStaff(staffId, userId, isActive: true);
        var user = BuildUser(userId, isActive: true);
        _staffRepo.SetupGetById(staffId, staff);
        _userRepo.SetupGetById(userId, user);

        await CreateHandler().Handle(new SetStaffActiveCommand(staffId, false), default);

        staff.IsActive.Should().BeFalse();
        user.IsActive.Should().BeFalse();
        _staffRepo.Verify(r => r.Update(staff), Times.Once);
        _userRepo.Verify(r => r.Update(user), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Activates_staff_and_user_when_setting_active()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var staff = BuildStaff(staffId, userId, isActive: false);
        var user = BuildUser(userId, isActive: false);
        _staffRepo.SetupGetById(staffId, staff);
        _userRepo.SetupGetById(userId, user);

        await CreateHandler().Handle(new SetStaffActiveCommand(staffId, true), default);

        staff.IsActive.Should().BeTrue();
        user.IsActive.Should().BeTrue();
        _staffRepo.Verify(r => r.Update(staff), Times.Once);
        _userRepo.Verify(r => r.Update(user), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Persists_change_even_when_state_already_matches_requested_value()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var staff = BuildStaff(staffId, userId, isActive: true);
        var user = BuildUser(userId, isActive: true);
        _staffRepo.SetupGetById(staffId, staff);
        _userRepo.SetupGetById(userId, user);

        await CreateHandler().Handle(new SetStaffActiveCommand(staffId, true), default);

        staff.IsActive.Should().BeTrue();
        user.IsActive.Should().BeTrue();
        _staffRepo.Verify(r => r.Update(staff), Times.Once);
        _userRepo.Verify(r => r.Update(user), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
