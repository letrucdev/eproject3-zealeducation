using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Faculties.Commands.CreateFaculty;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Faculties;

public class CreateFacultyCommandHandlerTests
{
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<IRepository<Faculty>> _facultyRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();
    private readonly Mock<IPasswordHasher> _hasher = new();

    private CreateFacultyCommandHandler CreateHandler() =>
        new(_userRepo.Object, _staffRepo.Object, _facultyRepo.Object, _uow.Object, _hasher.Object);

    private static CreateFacultyCommand Cmd(
        string username = "john.doe",
        string password = "Password1",
        string fullName = "John Doe",
        string email = "john@example.com",
        string phone = "0123456789",
        DateOnly? dob = null,
        Gender gender = Gender.Male,
        string position = "Lecturer",
        string department = "Computer Science",
        DateOnly? joinedDate = null,
        string facultyCode = "F-001",
        string qualification = "PhD",
        string specialization = "AI",
        int experienceYears = 5) => new(
            username,
            password,
            fullName,
            email,
            phone,
            dob ?? new DateOnly(1990, 1, 1),
            gender,
            position,
            department,
            joinedDate,
            facultyCode,
            qualification,
            specialization,
            experienceYears);

    [Fact]
    public async Task Throws_ConflictException_when_username_is_already_in_use()
    {
        _userRepo.SetupFind(new[]
        {
            new UserAccount
            {
                Id = Guid.NewGuid(),
                Username = "john.doe",
                Email = "other@example.com",
                Phone = "9999999999"
            }
        });

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Username is already in use.");
        _userRepo.Verify(r => r.AddAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_email_is_already_in_use()
    {
        _userRepo.SetupFind(new[]
        {
            new UserAccount
            {
                Id = Guid.NewGuid(),
                Username = "different.user",
                Email = "john@example.com",
                Phone = "9999999999"
            }
        });

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Email is already in use.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_phone_is_already_in_use()
    {
        _userRepo.SetupFind(new[]
        {
            new UserAccount
            {
                Id = Guid.NewGuid(),
                Username = "different.user",
                Email = "different@example.com",
                Phone = "0123456789"
            }
        });

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Phone number is already in use.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_faculty_code_is_already_in_use()
    {
        _userRepo.SetupFind(Array.Empty<UserAccount>());
        _facultyRepo.SetupFind(new[]
        {
            new Faculty { Id = Guid.NewGuid(), FacultyCode = "F-001" }
        });

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Faculty code is already in use.");
        _facultyRepo.Verify(r => r.AddAsync(It.IsAny<Faculty>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Creates_user_staff_and_faculty_and_returns_response()
    {
        _userRepo.SetupFind(Array.Empty<UserAccount>());
        _facultyRepo.SetupFind(Array.Empty<Faculty>());
        _userRepo.SetupAdd();
        _staffRepo.SetupAdd();
        _facultyRepo.SetupAdd();
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("HASHED");

        var joined = new DateOnly(2024, 6, 1);
        var response = await CreateHandler().Handle(
            Cmd(joinedDate: joined, experienceYears: 7),
            default);

        response.UserAccountId.Should().NotBe(Guid.Empty);
        response.StaffId.Should().NotBe(Guid.Empty);
        response.FacultyId.Should().NotBe(Guid.Empty);
        response.Username.Should().Be("john.doe");
        response.FullName.Should().Be("John Doe");
        response.Email.Should().Be("john@example.com");
        response.Phone.Should().Be("0123456789");
        response.Role.Should().Be(UserRole.Faculty);
        response.Position.Should().Be("Lecturer");
        response.Department.Should().Be("Computer Science");
        response.JoinedDate.Should().Be(joined);
        response.FacultyCode.Should().Be("F-001");
        response.Qualification.Should().Be("PhD");
        response.Specialization.Should().Be("AI");
        response.ExperienceYears.Should().Be(7);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Trims_string_inputs_before_saving()
    {
        _userRepo.SetupFind(Array.Empty<UserAccount>());
        _facultyRepo.SetupFind(Array.Empty<Faculty>());
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("HASHED");

        UserAccount? capturedUser = null;
        Staff? capturedStaff = null;
        Faculty? capturedFaculty = null;

        _userRepo.Setup(r => r.AddAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()))
            .Callback<UserAccount, CancellationToken>((u, _) => capturedUser = u)
            .ReturnsAsync((UserAccount u, CancellationToken _) => u);
        _staffRepo.Setup(r => r.AddAsync(It.IsAny<Staff>(), It.IsAny<CancellationToken>()))
            .Callback<Staff, CancellationToken>((s, _) => capturedStaff = s)
            .ReturnsAsync((Staff s, CancellationToken _) => s);
        _facultyRepo.Setup(r => r.AddAsync(It.IsAny<Faculty>(), It.IsAny<CancellationToken>()))
            .Callback<Faculty, CancellationToken>((f, _) => capturedFaculty = f)
            .ReturnsAsync((Faculty f, CancellationToken _) => f);

        await CreateHandler().Handle(
            Cmd(
                username: "  john.doe  ",
                fullName: "  John Doe  ",
                email: "  john@example.com  ",
                phone: "  0123456789  ",
                position: "  Lecturer  ",
                department: "  Computer Science  ",
                facultyCode: "  F-001  ",
                qualification: "  PhD  ",
                specialization: "  AI  "),
            default);

        capturedUser.Should().NotBeNull();
        capturedUser!.Username.Should().Be("john.doe");
        capturedUser.FullName.Should().Be("John Doe");
        capturedUser.Email.Should().Be("john@example.com");
        capturedUser.Phone.Should().Be("0123456789");

        capturedStaff.Should().NotBeNull();
        capturedStaff!.Position.Should().Be("Lecturer");
        capturedStaff.Department.Should().Be("Computer Science");

        capturedFaculty.Should().NotBeNull();
        capturedFaculty!.FacultyCode.Should().Be("F-001");
        capturedFaculty.Qualification.Should().Be("PhD");
        capturedFaculty.Specialization.Should().Be("AI");
    }

    [Fact]
    public async Task Sets_default_user_account_flags_and_role()
    {
        _userRepo.SetupFind(Array.Empty<UserAccount>());
        _facultyRepo.SetupFind(Array.Empty<Faculty>());
        _hasher.Setup(h => h.Hash("Password1")).Returns("HASHED");

        UserAccount? captured = null;
        _userRepo.Setup(r => r.AddAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()))
            .Callback<UserAccount, CancellationToken>((u, _) => captured = u)
            .ReturnsAsync((UserAccount u, CancellationToken _) => u);

        await CreateHandler().Handle(Cmd(), default);

        captured.Should().NotBeNull();
        captured!.Role.Should().Be(UserRole.Faculty);
        captured.IsActive.Should().BeTrue();
        captured.MustChangePassword.Should().BeTrue();
        captured.FailedLoginCount.Should().Be(0);
        captured.PasswordHash.Should().Be("HASHED");
    }

    [Fact]
    public async Task Defaults_joined_date_to_today_when_not_provided()
    {
        _userRepo.SetupFind(Array.Empty<UserAccount>());
        _facultyRepo.SetupFind(Array.Empty<Faculty>());
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("HASHED");

        Staff? captured = null;
        _staffRepo.Setup(r => r.AddAsync(It.IsAny<Staff>(), It.IsAny<CancellationToken>()))
            .Callback<Staff, CancellationToken>((s, _) => captured = s)
            .ReturnsAsync((Staff s, CancellationToken _) => s);

        await CreateHandler().Handle(Cmd(joinedDate: null), default);

        captured.Should().NotBeNull();
        captured!.JoinedDate.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
        captured.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Links_staff_to_user_and_faculty_to_staff()
    {
        _userRepo.SetupFind(Array.Empty<UserAccount>());
        _facultyRepo.SetupFind(Array.Empty<Faculty>());
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("HASHED");

        UserAccount? capturedUser = null;
        Staff? capturedStaff = null;
        Faculty? capturedFaculty = null;

        _userRepo.Setup(r => r.AddAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()))
            .Callback<UserAccount, CancellationToken>((u, _) => capturedUser = u)
            .ReturnsAsync((UserAccount u, CancellationToken _) => u);
        _staffRepo.Setup(r => r.AddAsync(It.IsAny<Staff>(), It.IsAny<CancellationToken>()))
            .Callback<Staff, CancellationToken>((s, _) => capturedStaff = s)
            .ReturnsAsync((Staff s, CancellationToken _) => s);
        _facultyRepo.Setup(r => r.AddAsync(It.IsAny<Faculty>(), It.IsAny<CancellationToken>()))
            .Callback<Faculty, CancellationToken>((f, _) => capturedFaculty = f)
            .ReturnsAsync((Faculty f, CancellationToken _) => f);

        await CreateHandler().Handle(Cmd(), default);

        capturedStaff!.UserAccountId.Should().Be(capturedUser!.Id);
        capturedFaculty!.StaffId.Should().Be(capturedStaff.Id);
    }
}
