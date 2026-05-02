using ZealEducation.Application.Features.Faculties.Queries.GetFaculties;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Queries.Faculties;

[Collection(DatabaseCollection.Name)]
public class GetFacultiesQueryHandlerTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;

    public GetFacultiesQueryHandlerTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private GetFacultiesQueryHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new GetFacultiesQueryHandler(new GenericRepository<Faculty>(ctx));
    }

    private static UserAccount NewUser(string username, string fullName, bool active = true) => new()
    {
        Id = Guid.NewGuid(),
        Username = username,
        PasswordHash = "hash",
        FullName = fullName,
        Email = $"{username}@example.com",
        Phone = $"0{Random.Shared.Next(100000000, 999999999)}",
        Dob = new DateOnly(1990, 1, 1),
        Gender = Gender.Male,
        Role = UserRole.Faculty,
        IsActive = active
    };

    private static Staff NewStaff(Guid userAccountId, bool active = true) => new()
    {
        Id = Guid.NewGuid(),
        UserAccountId = userAccountId,
        Position = "Lecturer",
        Department = "Engineering",
        JoinedDate = new DateOnly(2020, 1, 1),
        IsActive = active
    };

    private static Faculty NewFaculty(
        Guid staffId,
        string code,
        string qualification = "MSc",
        string specialization = "Computer Science",
        int experience = 5) => new()
        {
            Id = Guid.NewGuid(),
            StaffId = staffId,
            FacultyCode = code,
            Qualification = qualification,
            Specialization = specialization,
            ExperienceYears = experience
        };

    private async Task<Faculty> SeedFacultyAsync(
        string username,
        string fullName,
        string code,
        string qualification = "MSc",
        string specialization = "Computer Science",
        int experience = 5,
        bool userActive = true,
        bool staffActive = true)
    {
        await using var ctx = _fixture.CreateDbContext();
        var user = NewUser(username, fullName, userActive);
        var staff = NewStaff(user.Id, staffActive);
        var faculty = NewFaculty(staff.Id, code, qualification, specialization, experience);
        ctx.UserAccounts.Add(user);
        ctx.Staff.Add(staff);
        ctx.Faculties.Add(faculty);
        await ctx.SaveChangesAsync();
        return faculty;
    }

    [Fact]
    public async Task Returns_empty_list_when_no_faculties_exist()
    {
        var result = await CreateHandler().Handle(new GetFacultiesQuery(), default);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Returns_paginated_faculties_with_default_paging()
    {
        for (var i = 1; i <= 15; i++)
            await SeedFacultyAsync($"user{i:D2}", $"Name {i:D2}", $"FAC{i:D3}");

        var result = await CreateHandler().Handle(new GetFacultiesQuery(), default);

        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task Filters_by_search_in_full_name_case_insensitive()
    {
        await SeedFacultyAsync("alice", "Alice Smith", "FAC001");
        await SeedFacultyAsync("bob", "Bob Jones", "FAC002");
        await SeedFacultyAsync("carol", "Carol Alicia", "FAC003");

        var result = await CreateHandler().Handle(new GetFacultiesQuery(Search: "ALICE"), default);

        result.Items.Should().HaveCount(1);
        result.Items.Select(f => f.FullName).Should().BeEquivalentTo("Alice Smith");
    }

    [Fact]
    public async Task Filters_by_search_in_faculty_code()
    {
        await SeedFacultyAsync("alice", "Alice", "ENG001");
        await SeedFacultyAsync("bob", "Bob", "MAT002");

        var result = await CreateHandler().Handle(new GetFacultiesQuery(Search: "eng"), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().FacultyCode.Should().Be("ENG001");
    }

    [Fact]
    public async Task Filters_by_search_in_specialization()
    {
        await SeedFacultyAsync("alice", "Alice", "FAC001", specialization: "Mathematics");
        await SeedFacultyAsync("bob", "Bob", "FAC002", specialization: "Physics");

        var result = await CreateHandler().Handle(new GetFacultiesQuery(Search: "math"), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().FullName.Should().Be("Alice");
    }

    [Fact]
    public async Task Filters_by_search_in_email()
    {
        await SeedFacultyAsync("uniqueuser", "Alice", "FAC001");
        await SeedFacultyAsync("other", "Bob", "FAC002");

        var result = await CreateHandler().Handle(new GetFacultiesQuery(Search: "uniqueuser@"), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().FullName.Should().Be("Alice");
    }

    [Fact]
    public async Task Orders_results_by_full_name_ascending()
    {
        await SeedFacultyAsync("a", "Charlie", "FAC001");
        await SeedFacultyAsync("b", "Alpha", "FAC002");
        await SeedFacultyAsync("c", "Bravo", "FAC003");

        var result = await CreateHandler().Handle(new GetFacultiesQuery(), default);

        result.Items.Select(f => f.FullName).Should().ContainInOrder("Alpha", "Bravo", "Charlie");
    }

    [Fact]
    public async Task Clamps_page_below_1_to_first_page()
    {
        for (var i = 1; i <= 3; i++)
            await SeedFacultyAsync($"u{i}", $"Name {i}", $"FAC{i:D3}");

        var result = await CreateHandler().Handle(new GetFacultiesQuery(Page: -5), default);

        result.PageNumber.Should().Be(1);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Clamps_page_size_above_100_to_100()
    {
        await SeedFacultyAsync("u1", "Solo", "FAC001");

        var result = await CreateHandler().Handle(new GetFacultiesQuery(PageSize: 500), default);

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Returns_second_page_correctly()
    {
        for (var i = 1; i <= 12; i++)
            await SeedFacultyAsync($"u{i:D2}", $"Name {i:D2}", $"FAC{i:D3}");

        var result = await CreateHandler().Handle(new GetFacultiesQuery(Page: 2, PageSize: 10), default);

        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(2);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Maps_all_dto_fields_correctly()
    {
        var faculty = await SeedFacultyAsync(
            username: "mappeduser",
            fullName: "Mapped User",
            code: "MAP100",
            qualification: "PhD",
            specialization: "Robotics",
            experience: 10);

        var result = await CreateHandler().Handle(new GetFacultiesQuery(), default);

        var dto = result.Items.Single();
        dto.FacultyId.Should().Be(faculty.Id);
        dto.FacultyCode.Should().Be("MAP100");
        dto.FullName.Should().Be("Mapped User");
        dto.Email.Should().Be("mappeduser@example.com");
        dto.Phone.Should().NotBeNullOrEmpty();
        dto.Qualification.Should().Be("PhD");
        dto.Specialization.Should().Be("Robotics");
        dto.ExperienceYears.Should().Be(10);
        dto.IsActive.Should().BeTrue();
        dto.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task IsActive_false_when_user_or_staff_inactive()
    {
        await SeedFacultyAsync("u1", "Both Active", "FAC001");
        await SeedFacultyAsync("u2", "User Inactive", "FAC002", userActive: false);
        await SeedFacultyAsync("u3", "Staff Inactive", "FAC003", staffActive: false);

        var result = await CreateHandler().Handle(new GetFacultiesQuery(), default);

        var byName = result.Items.ToDictionary(f => f.FullName, f => f.IsActive);
        byName["Both Active"].Should().BeTrue();
        byName["User Inactive"].Should().BeFalse();
        byName["Staff Inactive"].Should().BeFalse();
    }
}
