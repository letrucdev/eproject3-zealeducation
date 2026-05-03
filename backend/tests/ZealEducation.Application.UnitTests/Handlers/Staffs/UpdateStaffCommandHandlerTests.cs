using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Staffs.Commands.UpdateStaff;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Staffs;

public class UpdateStaffCommandHandlerTests
{
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private UpdateStaffCommandHandler CreateHandler() =>
        new(_staffRepo.Object, _userRepo.Object, _uow.Object);

    private static UpdateStaffCommand Cmd(
        Guid? staffId = null,
        string fullName = "Alice Doe",
        string email = "alice@example.com",
        string phone = "0123456789",
        DateOnly? dob = null,
        Gender gender = Gender.Female,
        UserRole role = UserRole.Counselor,
        string position = "Counselor",
        string department = "Admissions",
        DateOnly? joinedDate = null,
        bool isActive = true) => new(
            staffId ?? Guid.NewGuid(),
            fullName,
            email,
            phone,
            dob ?? new DateOnly(1995, 1, 1),
            gender,
            role,
            position,
            department,
            joinedDate ?? new DateOnly(2020, 1, 1),
            isActive);

    private static Staff ExistingStaff(Guid id, Guid userAccountId) => new()
    {
        Id = id,
        UserAccountId = userAccountId,
        Position = "Old Position",
        Department = "Old Department",
        JoinedDate = new DateOnly(2018, 1, 1),
        IsActive = true
    };

    private static UserAccount ExistingUser(
        Guid id,
        string email = "old@example.com",
        string phone = "9999999999",
        string fullName = "Old Name") => new()
        {
            Id = id,
            Username = "alice.doe",
            PasswordHash = "$hash$",
            FullName = fullName,
            Email = email,
            Phone = phone,
            Dob = new DateOnly(1990, 1, 1),
            Gender = Gender.Female,
            Role = UserRole.Counselor,
            IsActive = true
        };

    [Fact]
    public async Task Throws_NotFoundException_when_staff_does_not_exist()
    {
        var staffId = Guid.NewGuid();
        _staffRepo.SetupGetById(staffId, null);

        var act = async () => await CreateHandler().Handle(Cmd(staffId), default);

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
        _staffRepo.SetupGetById(staffId, ExistingStaff(staffId, userId));
        _userRepo.SetupGetById(userId, null);

        var act = async () => await CreateHandler().Handle(Cmd(staffId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _staffRepo.Verify(r => r.Update(It.IsAny<Staff>()), Times.Never);
        _userRepo.Verify(r => r.Update(It.IsAny<UserAccount>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_email_is_already_used_by_another_user()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _staffRepo.SetupGetById(staffId, ExistingStaff(staffId, userId));
        _userRepo.SetupGetById(userId, ExistingUser(userId));
        _userRepo.SetupFind(new[]
        {
            new UserAccount { Id = Guid.NewGuid(), Email = "alice@example.com", Phone = "0000000000" }
        });

        var act = async () => await CreateHandler().Handle(Cmd(staffId), default);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Email is already in use.");
        _staffRepo.Verify(r => r.Update(It.IsAny<Staff>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_phone_is_already_used_by_another_user()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _staffRepo.SetupGetById(staffId, ExistingStaff(staffId, userId));
        _userRepo.SetupGetById(userId, ExistingUser(userId));
        _userRepo.SetupFind(new[]
        {
            new UserAccount { Id = Guid.NewGuid(), Email = "other@example.com", Phone = "0123456789" }
        });

        var act = async () => await CreateHandler().Handle(Cmd(staffId), default);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Phone number is already in use.");
    }

    [Fact]
    public async Task Updates_user_and_staff_when_inputs_are_valid()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var staff = ExistingStaff(staffId, userId);
        var user = ExistingUser(userId);
        _staffRepo.SetupGetById(staffId, staff);
        _userRepo.SetupGetById(userId, user);
        _userRepo.SetupFind(Array.Empty<UserAccount>());

        var joined = new DateOnly(2021, 6, 1);
        await CreateHandler().Handle(
            Cmd(staffId, fullName: "New Name", email: "new@example.com", phone: "0987654321",
                dob: new DateOnly(1992, 2, 2), gender: Gender.Male, role: UserRole.Incharge,
                position: "Manager", department: "Operations", joinedDate: joined, isActive: false),
            default);

        user.FullName.Should().Be("New Name");
        user.Email.Should().Be("new@example.com");
        user.Phone.Should().Be("0987654321");
        user.Dob.Should().Be(new DateOnly(1992, 2, 2));
        user.Gender.Should().Be(Gender.Male);
        user.Role.Should().Be(UserRole.Incharge);
        user.IsActive.Should().BeFalse();

        staff.Position.Should().Be("Manager");
        staff.Department.Should().Be("Operations");
        staff.JoinedDate.Should().Be(joined);

        _userRepo.Verify(r => r.Update(user), Times.Once);
        _staffRepo.Verify(r => r.Update(staff), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Trims_text_inputs_before_saving()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var staff = ExistingStaff(staffId, userId);
        var user = ExistingUser(userId);
        _staffRepo.SetupGetById(staffId, staff);
        _userRepo.SetupGetById(userId, user);
        _userRepo.SetupFind(Array.Empty<UserAccount>());

        await CreateHandler().Handle(
            Cmd(staffId, fullName: "  Trimmed Name  ", email: "  trimmed@example.com  ",
                phone: "  0123456789  ", position: "  Counselor  ", department: "  Admissions  "),
            default);

        user.FullName.Should().Be("Trimmed Name");
        user.Email.Should().Be("trimmed@example.com");
        user.Phone.Should().Be("0123456789");
        staff.Position.Should().Be("Counselor");
        staff.Department.Should().Be("Admissions");
    }

    [Fact]
    public async Task Does_not_treat_unchanged_email_or_phone_as_duplicate()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var staff = ExistingStaff(staffId, userId);
        var user = ExistingUser(userId, email: "alice@example.com", phone: "0123456789");
        _staffRepo.SetupGetById(staffId, staff);
        _userRepo.SetupGetById(userId, user);
        _userRepo.SetupFind(Array.Empty<UserAccount>());

        await CreateHandler().Handle(Cmd(staffId), default);

        _userRepo.Verify(r => r.Update(user), Times.Once);
        _staffRepo.Verify(r => r.Update(staff), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
