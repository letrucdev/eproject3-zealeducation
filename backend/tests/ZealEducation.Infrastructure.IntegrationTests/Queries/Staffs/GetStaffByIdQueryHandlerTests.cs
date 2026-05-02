using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Staffs.Queries.GetStaffById;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Queries.Staffs;

[Collection(DatabaseCollection.Name)]
public class GetStaffByIdQueryHandlerTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;

    public GetStaffByIdQueryHandlerTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private GetStaffByIdQueryHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new GetStaffByIdQueryHandler(
            new GenericRepository<Staff>(ctx),
            new GenericRepository<UserAccount>(ctx),
            new GenericRepository<Faculty>(ctx));
    }

    private static UserAccount NewUser(string suffix, UserRole role = UserRole.Counselor) => new()
    {
        Id = Guid.NewGuid(),
        Username = $"byid_{suffix}",
        PasswordHash = "hash",
        FullName = $"ById User {suffix}",
        Email = $"byid_{suffix}@example.com",
        Phone = $"0966{suffix}",
        Dob = new DateOnly(1992, 4, 4),
        Gender = Gender.Female,
        Role = role,
        IsActive = true
    };

    [Fact]
    public async Task Throws_NotFoundException_when_staff_id_missing()
    {
        var act = async () =>
            await CreateHandler().Handle(new GetStaffByIdQuery(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Returns_dto_with_user_fields_populated_when_no_faculty()
    {
        var user = NewUser("a01", UserRole.Counselor);
        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            UserAccountId = user.Id,
            Position = "Counselor",
            Department = "Admissions",
            JoinedDate = new DateOnly(2024, 2, 1)
        };

        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.UserAccounts.Add(user);
            await ctx.SaveChangesAsync();
            ctx.Staff.Add(staff);
            await ctx.SaveChangesAsync();
        }

        var dto = await CreateHandler().Handle(new GetStaffByIdQuery(staff.Id), default);

        dto.StaffId.Should().Be(staff.Id);
        dto.UserAccountId.Should().Be(user.Id);
        dto.Username.Should().Be(user.Username);
        dto.FullName.Should().Be(user.FullName);
        dto.Email.Should().Be(user.Email);
        dto.Phone.Should().Be(user.Phone);
        dto.Dob.Should().Be(user.Dob);
        dto.Gender.Should().Be(Gender.Female);
        dto.Role.Should().Be(UserRole.Counselor);
        dto.IsActive.Should().BeTrue();
        dto.Position.Should().Be("Counselor");
        dto.Department.Should().Be("Admissions");
        dto.JoinedDate.Should().Be(new DateOnly(2024, 2, 1));
        dto.FacultyId.Should().BeNull();
        dto.FacultyCode.Should().BeNull();
        dto.Qualification.Should().BeNull();
        dto.Specialization.Should().BeNull();
        dto.ExperienceYears.Should().BeNull();
    }

    [Fact]
    public async Task Returns_faculty_data_when_staff_is_faculty()
    {
        var user = NewUser("b02", UserRole.Faculty);
        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            UserAccountId = user.Id,
            Position = "Lecturer",
            Department = "Computer Science",
            JoinedDate = new DateOnly(2022, 8, 1)
        };
        var faculty = new Faculty
        {
            Id = Guid.NewGuid(),
            StaffId = staff.Id,
            FacultyCode = "FAC001",
            Qualification = "PhD",
            Specialization = "AI",
            ExperienceYears = 10
        };

        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.UserAccounts.Add(user);
            await ctx.SaveChangesAsync();
            ctx.Staff.Add(staff);
            await ctx.SaveChangesAsync();
            ctx.Faculties.Add(faculty);
            await ctx.SaveChangesAsync();
        }

        var dto = await CreateHandler().Handle(new GetStaffByIdQuery(staff.Id), default);

        dto.StaffId.Should().Be(staff.Id);
        dto.Role.Should().Be(UserRole.Faculty);
        dto.FacultyId.Should().Be(faculty.Id);
        dto.FacultyCode.Should().Be("FAC001");
        dto.Qualification.Should().Be("PhD");
        dto.Specialization.Should().Be("AI");
        dto.ExperienceYears.Should().Be(10);
    }
}
