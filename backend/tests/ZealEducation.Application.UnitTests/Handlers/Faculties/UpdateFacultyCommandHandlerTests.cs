using System.Linq.Expressions;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Faculties.Commands.UpdateFaculty;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Faculties;

public class UpdateFacultyCommandHandlerTests
{
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<IRepository<Faculty>> _facultyRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private UpdateFacultyCommandHandler CreateHandler() =>
        new(_staffRepo.Object, _userRepo.Object, _facultyRepo.Object, _uow.Object);

    private static UpdateFacultyCommand Cmd(
        Guid? staffId = null,
        string fullName = "John Doe",
        string email = "john@example.com",
        string phone = "0123456789",
        DateOnly? dob = null,
        Gender gender = Gender.Male,
        string position = "Lecturer",
        string department = "Computer Science",
        DateOnly? joinedDate = null,
        bool isActive = true,
        string facultyCode = "F-001",
        string qualification = "PhD",
        string specialization = "AI",
        int experienceYears = 5) => new(
            staffId ?? Guid.NewGuid(),
            fullName,
            email,
            phone,
            dob ?? new DateOnly(1990, 1, 1),
            gender,
            position,
            department,
            joinedDate ?? new DateOnly(2024, 6, 1),
            isActive,
            facultyCode,
            qualification,
            specialization,
            experienceYears);

    private static Staff ExistingStaff(Guid staffId, Guid userAccountId) => new()
    {
        Id = staffId,
        UserAccountId = userAccountId,
        Position = "Old Position",
        Department = "Old Department",
        JoinedDate = new DateOnly(2020, 1, 1),
        IsActive = true
    };

    private static UserAccount ExistingUser(Guid userId, UserRole role = UserRole.Faculty) => new()
    {
        Id = userId,
        Username = "old.user",
        FullName = "Old Name",
        Email = "old@example.com",
        Phone = "9999999999",
        Dob = new DateOnly(1985, 5, 5),
        Gender = Gender.Female,
        Role = role,
        IsActive = true
    };

    private static Faculty ExistingFaculty(Guid facultyId, Guid staffId) => new()
    {
        Id = facultyId,
        StaffId = staffId,
        FacultyCode = "OLD-001",
        Qualification = "Old Qualification",
        Specialization = "Old Specialization",
        ExperienceYears = 1
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
        _facultyRepo.Verify(r => r.Update(It.IsAny<Faculty>()), Times.Never);
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
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_user_role_is_not_faculty()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _staffRepo.SetupGetById(staffId, ExistingStaff(staffId, userId));
        _userRepo.SetupGetById(userId, ExistingUser(userId, UserRole.AccountsStaff));

        var act = async () => await CreateHandler().Handle(Cmd(staffId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Staff is not a faculty member.");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_faculty_record_is_missing()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _staffRepo.SetupGetById(staffId, ExistingStaff(staffId, userId));
        _userRepo.SetupGetById(userId, ExistingUser(userId));
        _facultyRepo.SetupFind(Array.Empty<Faculty>());

        var act = async () => await CreateHandler().Handle(Cmd(staffId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_ConflictException_when_email_is_already_in_use_by_another_user()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        _staffRepo.SetupGetById(staffId, ExistingStaff(staffId, userId));
        _userRepo.SetupGetById(userId, ExistingUser(userId));
        _facultyRepo.SetupFind(new[] { ExistingFaculty(facultyId, staffId) });
        _userRepo.SetupFind(new[]
        {
            new UserAccount
            {
                Id = Guid.NewGuid(),
                Email = "john@example.com",
                Phone = "1111111111"
            }
        });

        var act = async () => await CreateHandler().Handle(Cmd(staffId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Email is already in use.");
        _staffRepo.Verify(r => r.Update(It.IsAny<Staff>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_phone_is_already_in_use_by_another_user()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        _staffRepo.SetupGetById(staffId, ExistingStaff(staffId, userId));
        _userRepo.SetupGetById(userId, ExistingUser(userId));
        _facultyRepo.SetupFind(new[] { ExistingFaculty(facultyId, staffId) });
        _userRepo.SetupFind(new[]
        {
            new UserAccount
            {
                Id = Guid.NewGuid(),
                Email = "different@example.com",
                Phone = "0123456789"
            }
        });

        var act = async () => await CreateHandler().Handle(Cmd(staffId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Phone number is already in use.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_faculty_code_is_already_in_use_by_another_faculty()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        _staffRepo.SetupGetById(staffId, ExistingStaff(staffId, userId));
        _userRepo.SetupGetById(userId, ExistingUser(userId));
        _userRepo.SetupFind(Array.Empty<UserAccount>());
        _facultyRepo.SetupSequence(r => r.FindAsync(
                It.IsAny<Expression<Func<Faculty, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Faculty> { ExistingFaculty(facultyId, staffId) })
            .ReturnsAsync(new List<Faculty> { new() { Id = Guid.NewGuid(), FacultyCode = "F-001" } });

        var act = async () => await CreateHandler().Handle(Cmd(staffId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Faculty code is already in use.");
        _facultyRepo.Verify(r => r.Update(It.IsAny<Faculty>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Updates_user_staff_and_faculty_when_command_is_valid()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        var staff = ExistingStaff(staffId, userId);
        var user = ExistingUser(userId);
        var faculty = ExistingFaculty(facultyId, staffId);

        _staffRepo.SetupGetById(staffId, staff);
        _userRepo.SetupGetById(userId, user);
        _userRepo.SetupFind(Array.Empty<UserAccount>());
        _facultyRepo.SetupSequence(r => r.FindAsync(
                It.IsAny<Expression<Func<Faculty, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Faculty> { faculty })
            .ReturnsAsync(new List<Faculty>());

        var newJoined = new DateOnly(2025, 3, 15);
        var newDob = new DateOnly(1991, 7, 7);
        await CreateHandler().Handle(
            Cmd(staffId,
                fullName: "Brand New Name",
                email: "new@example.com",
                phone: "0987654321",
                dob: newDob,
                gender: Gender.Other,
                position: "Senior Lecturer",
                department: "Math",
                joinedDate: newJoined,
                isActive: false,
                facultyCode: "F-NEW",
                qualification: "MSc",
                specialization: "ML",
                experienceYears: 12),
            default);

        user.FullName.Should().Be("Brand New Name");
        user.Email.Should().Be("new@example.com");
        user.Phone.Should().Be("0987654321");
        user.Dob.Should().Be(newDob);
        user.Gender.Should().Be(Gender.Other);
        user.IsActive.Should().BeFalse();

        staff.Position.Should().Be("Senior Lecturer");
        staff.Department.Should().Be("Math");
        staff.JoinedDate.Should().Be(newJoined);

        faculty.FacultyCode.Should().Be("F-NEW");
        faculty.Qualification.Should().Be("MSc");
        faculty.Specialization.Should().Be("ML");
        faculty.ExperienceYears.Should().Be(12);

        _userRepo.Verify(r => r.Update(user), Times.Once);
        _staffRepo.Verify(r => r.Update(staff), Times.Once);
        _facultyRepo.Verify(r => r.Update(faculty), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Trims_string_inputs_before_saving()
    {
        var staffId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        var staff = ExistingStaff(staffId, userId);
        var user = ExistingUser(userId);
        var faculty = ExistingFaculty(facultyId, staffId);

        _staffRepo.SetupGetById(staffId, staff);
        _userRepo.SetupGetById(userId, user);
        _userRepo.SetupFind(Array.Empty<UserAccount>());
        _facultyRepo.SetupSequence(r => r.FindAsync(
                It.IsAny<Expression<Func<Faculty, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Faculty> { faculty })
            .ReturnsAsync(new List<Faculty>());

        await CreateHandler().Handle(
            Cmd(staffId,
                fullName: "  John Doe  ",
                email: "  john@example.com  ",
                phone: "  0123456789  ",
                position: "  Lecturer  ",
                department: "  Computer Science  ",
                facultyCode: "  F-001  ",
                qualification: "  PhD  ",
                specialization: "  AI  "),
            default);

        user.FullName.Should().Be("John Doe");
        user.Email.Should().Be("john@example.com");
        user.Phone.Should().Be("0123456789");
        staff.Position.Should().Be("Lecturer");
        staff.Department.Should().Be("Computer Science");
        faculty.FacultyCode.Should().Be("F-001");
        faculty.Qualification.Should().Be("PhD");
        faculty.Specialization.Should().Be("AI");
    }
}
