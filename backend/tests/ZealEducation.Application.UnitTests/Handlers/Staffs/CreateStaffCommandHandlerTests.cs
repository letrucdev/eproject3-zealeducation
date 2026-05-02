using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Staffs.Commands.CreateStaff;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Staffs;

public class CreateStaffCommandHandlerTests
{
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();
    private readonly Mock<IPasswordHasher> _hasher = new();

    private CreateStaffCommandHandler CreateHandler() =>
        new(_userRepo.Object, _staffRepo.Object, _uow.Object, _hasher.Object);

    private static CreateStaffCommand Cmd(
        string username = "alice.doe",
        string password = "Pass1234",
        string fullName = "Alice Doe",
        string email = "alice@example.com",
        string phone = "0123456789",
        DateOnly? dob = null,
        Gender gender = Gender.Female,
        UserRole role = UserRole.Counselor,
        string position = "Counselor",
        string department = "Admissions",
        DateOnly? joinedDate = null) => new(
            username,
            password,
            fullName,
            email,
            phone,
            dob ?? new DateOnly(1995, 1, 1),
            gender,
            role,
            position,
            department,
            joinedDate);

    [Fact]
    public async Task Throws_ConflictException_when_username_is_already_in_use()
    {
        _userRepo.SetupFind(new[]
        {
            new UserAccount { Id = Guid.NewGuid(), Username = "alice.doe", Email = "other@example.com", Phone = "0000000000" }
        });

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Username is already in use.");
        _userRepo.Verify(r => r.AddAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
        _staffRepo.Verify(r => r.AddAsync(It.IsAny<Staff>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_email_is_already_in_use()
    {
        _userRepo.SetupFind(new[]
        {
            new UserAccount { Id = Guid.NewGuid(), Username = "other.user", Email = "alice@example.com", Phone = "0000000000" }
        });

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Email is already in use.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_phone_is_already_in_use()
    {
        _userRepo.SetupFind(new[]
        {
            new UserAccount { Id = Guid.NewGuid(), Username = "other.user", Email = "other@example.com", Phone = "0123456789" }
        });

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Phone number is already in use.");
    }

    [Fact]
    public async Task Hashes_password_and_persists_user_and_staff()
    {
        _userRepo.SetupFind(Array.Empty<UserAccount>());
        _hasher.Setup(h => h.Hash("Pass1234")).Returns("$hashed$");

        UserAccount? capturedUser = null;
        Staff? capturedStaff = null;
        _userRepo.Setup(r => r.AddAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()))
            .Callback<UserAccount, CancellationToken>((u, _) => capturedUser = u)
            .ReturnsAsync((UserAccount u, CancellationToken _) => u);
        _staffRepo.Setup(r => r.AddAsync(It.IsAny<Staff>(), It.IsAny<CancellationToken>()))
            .Callback<Staff, CancellationToken>((s, _) => capturedStaff = s)
            .ReturnsAsync((Staff s, CancellationToken _) => s);

        var response = await CreateHandler().Handle(Cmd(), default);

        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().Be("$hashed$");
        capturedUser.Username.Should().Be("alice.doe");
        capturedUser.Email.Should().Be("alice@example.com");
        capturedUser.Phone.Should().Be("0123456789");
        capturedUser.FullName.Should().Be("Alice Doe");
        capturedUser.Role.Should().Be(UserRole.Counselor);
        capturedUser.IsActive.Should().BeTrue();
        capturedUser.MustChangePassword.Should().BeTrue();
        capturedUser.FailedLoginCount.Should().Be(0);

        capturedStaff.Should().NotBeNull();
        capturedStaff!.UserAccountId.Should().Be(capturedUser.Id);
        capturedStaff.Position.Should().Be("Counselor");
        capturedStaff.Department.Should().Be("Admissions");
        capturedStaff.IsActive.Should().BeTrue();

        response.UserAccountId.Should().Be(capturedUser.Id);
        response.StaffId.Should().Be(capturedStaff.Id);
        response.Username.Should().Be("alice.doe");
        response.Email.Should().Be("alice@example.com");

        _hasher.Verify(h => h.Hash("Pass1234"), Times.Once);
        _userRepo.Verify(r => r.AddAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()), Times.Once);
        _staffRepo.Verify(r => r.AddAsync(It.IsAny<Staff>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Trims_text_inputs_before_saving()
    {
        _userRepo.SetupFind(Array.Empty<UserAccount>());
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("$hashed$");

        UserAccount? capturedUser = null;
        Staff? capturedStaff = null;
        _userRepo.Setup(r => r.AddAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()))
            .Callback<UserAccount, CancellationToken>((u, _) => capturedUser = u)
            .ReturnsAsync((UserAccount u, CancellationToken _) => u);
        _staffRepo.Setup(r => r.AddAsync(It.IsAny<Staff>(), It.IsAny<CancellationToken>()))
            .Callback<Staff, CancellationToken>((s, _) => capturedStaff = s)
            .ReturnsAsync((Staff s, CancellationToken _) => s);

        var cmd = Cmd(
            username: "  alice.doe  ",
            fullName: "  Alice Doe  ",
            email: "  alice@example.com  ",
            phone: "  0123456789  ",
            position: "  Counselor  ",
            department: "  Admissions  ");

        await CreateHandler().Handle(cmd, default);

        capturedUser!.Username.Should().Be("alice.doe");
        capturedUser.FullName.Should().Be("Alice Doe");
        capturedUser.Email.Should().Be("alice@example.com");
        capturedUser.Phone.Should().Be("0123456789");
        capturedStaff!.Position.Should().Be("Counselor");
        capturedStaff.Department.Should().Be("Admissions");
    }

    [Fact]
    public async Task Defaults_joined_date_to_today_when_not_provided()
    {
        _userRepo.SetupFind(Array.Empty<UserAccount>());
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("$hashed$");

        Staff? capturedStaff = null;
        _staffRepo.Setup(r => r.AddAsync(It.IsAny<Staff>(), It.IsAny<CancellationToken>()))
            .Callback<Staff, CancellationToken>((s, _) => capturedStaff = s)
            .ReturnsAsync((Staff s, CancellationToken _) => s);

        await CreateHandler().Handle(Cmd(joinedDate: null), default);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        capturedStaff!.JoinedDate.Should().Be(today);
    }

    [Fact]
    public async Task Uses_supplied_joined_date_when_provided()
    {
        _userRepo.SetupFind(Array.Empty<UserAccount>());
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("$hashed$");

        Staff? capturedStaff = null;
        _staffRepo.Setup(r => r.AddAsync(It.IsAny<Staff>(), It.IsAny<CancellationToken>()))
            .Callback<Staff, CancellationToken>((s, _) => capturedStaff = s)
            .ReturnsAsync((Staff s, CancellationToken _) => s);

        var joined = new DateOnly(2022, 5, 1);
        await CreateHandler().Handle(Cmd(joinedDate: joined), default);

        capturedStaff!.JoinedDate.Should().Be(joined);
    }
}
