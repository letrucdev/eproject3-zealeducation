using ZealEducation.Application.Features.Staffs.Queries.GetStaffs;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Queries.Staffs;

[Collection(DatabaseCollection.Name)]
public class GetStaffsQueryHandlerTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;

    public GetStaffsQueryHandlerTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private GetStaffsQueryHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new GetStaffsQueryHandler(
            new GenericRepository<Staff>(ctx),
            new GenericRepository<UserAccount>(ctx));
    }

    private static UserAccount NewUser(
        string suffix,
        string fullName = "Staff User",
        UserRole role = UserRole.Counselor,
        bool isActive = true,
        string? email = null,
        string? phone = null) => new()
        {
            Id = Guid.NewGuid(),
            Username = $"staff_{suffix}",
            PasswordHash = "hash",
            FullName = fullName,
            Email = email ?? $"staff_{suffix}@example.com",
            Phone = phone ?? $"0944{suffix}",
            Dob = new DateOnly(1990, 1, 1),
            Gender = Gender.Male,
            Role = role,
            IsActive = isActive
        };

    private static Staff NewStaff(
        Guid userAccountId,
        string position = "Counselor",
        string department = "Admissions",
        DateOnly? joinedDate = null) => new()
        {
            Id = Guid.NewGuid(),
            UserAccountId = userAccountId,
            Position = position,
            Department = department,
            JoinedDate = joinedDate ?? new DateOnly(2024, 1, 1)
        };

    private async Task<(UserAccount user, Staff staff)> SeedStaffAsync(
        string suffix,
        string fullName = "Staff User",
        UserRole role = UserRole.Counselor,
        bool isActive = true,
        string position = "Counselor",
        string department = "Admissions",
        DateOnly? joinedDate = null,
        string? email = null,
        string? phone = null)
    {
        var user = NewUser(suffix, fullName, role, isActive, email, phone);
        var staff = NewStaff(user.Id, position, department, joinedDate);
        await using var ctx = _fixture.CreateDbContext();
        ctx.UserAccounts.Add(user);
        await ctx.SaveChangesAsync();
        ctx.Staff.Add(staff);
        await ctx.SaveChangesAsync();
        return (user, staff);
    }

    [Fact]
    public async Task Returns_empty_list_when_no_staff_exists()
    {
        var result = await CreateHandler().Handle(new GetStaffsQuery(), default);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Returns_paginated_staffs_with_default_paging()
    {
        for (var i = 1; i <= 12; i++)
        {
            await SeedStaffAsync($"{i:D3}", fullName: $"User {i:D2}");
        }

        var result = await CreateHandler().Handle(new GetStaffsQuery(), default);

        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(12);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task Excludes_users_with_non_staff_role()
    {
        await SeedStaffAsync("a01", fullName: "Real Staff", role: UserRole.Counselor);

        var orphanUser = NewUser("orphan", fullName: "Candidate Person", role: UserRole.Candidate);
        var orphanStaff = NewStaff(orphanUser.Id, "Other", "Other");
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.UserAccounts.Add(orphanUser);
            await ctx.SaveChangesAsync();
            ctx.Staff.Add(orphanStaff);
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetStaffsQuery(), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().FullName.Should().Be("Real Staff");
    }

    [Fact]
    public async Task Filters_by_role()
    {
        await SeedStaffAsync("r01", fullName: "Counselor One", role: UserRole.Counselor);
        await SeedStaffAsync("r02", fullName: "Faculty One", role: UserRole.Faculty);
        await SeedStaffAsync("r03", fullName: "Accounts One", role: UserRole.AccountsStaff);

        var result = await CreateHandler().Handle(new GetStaffsQuery(Role: UserRole.Faculty), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().FullName.Should().Be("Faculty One");
    }

    [Fact]
    public async Task Filters_by_is_active()
    {
        await SeedStaffAsync("i01", fullName: "Active One", isActive: true);
        await SeedStaffAsync("i02", fullName: "Inactive One", isActive: false);

        var result = await CreateHandler().Handle(new GetStaffsQuery(IsActive: false), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().FullName.Should().Be("Inactive One");
    }

    [Fact]
    public async Task Filters_by_search_in_full_name_case_insensitive()
    {
        await SeedStaffAsync("s01", fullName: "Alice Walker");
        await SeedStaffAsync("s02", fullName: "Bob Smith");
        await SeedStaffAsync("s03", fullName: "Charlotte Brown");

        var result = await CreateHandler().Handle(new GetStaffsQuery(Search: "ALICE"), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().FullName.Should().Be("Alice Walker");
    }

    [Fact]
    public async Task Sorts_by_email_descending()
    {
        await SeedStaffAsync("e01", email: "alpha@example.com", phone: "0900100001");
        await SeedStaffAsync("e02", email: "bravo@example.com", phone: "0900100002");
        await SeedStaffAsync("e03", email: "charlie@example.com", phone: "0900100003");

        var result = await CreateHandler().Handle(
            new GetStaffsQuery(SortBy: "email", SortDirection: "desc"), default);

        result.Items.Select(i => i.Email).Should().ContainInOrder(
            "charlie@example.com", "bravo@example.com", "alpha@example.com");
    }

    [Fact]
    public async Task Sorts_by_joined_date_ascending()
    {
        await SeedStaffAsync("j01", fullName: "Older", joinedDate: new DateOnly(2020, 1, 1));
        await SeedStaffAsync("j02", fullName: "Newer", joinedDate: new DateOnly(2024, 6, 1));
        await SeedStaffAsync("j03", fullName: "Middle", joinedDate: new DateOnly(2022, 3, 1));

        var result = await CreateHandler().Handle(
            new GetStaffsQuery(SortBy: "joinedDate", SortDirection: "asc"), default);

        result.Items.Select(i => i.FullName).Should().ContainInOrder("Older", "Middle", "Newer");
    }

    [Fact]
    public async Task Defaults_to_full_name_ascending_when_sort_unspecified()
    {
        await SeedStaffAsync("d01", fullName: "Charlie");
        await SeedStaffAsync("d02", fullName: "Alpha");
        await SeedStaffAsync("d03", fullName: "Bravo");

        var result = await CreateHandler().Handle(new GetStaffsQuery(), default);

        result.Items.Select(i => i.FullName).Should().ContainInOrder("Alpha", "Bravo", "Charlie");
    }

    [Fact]
    public async Task Returns_second_page_correctly()
    {
        for (var i = 1; i <= 12; i++)
        {
            await SeedStaffAsync($"p{i:D3}", fullName: $"User {i:D2}");
        }

        var result = await CreateHandler().Handle(new GetStaffsQuery(Page: 2, PageSize: 10), default);

        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(2);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Clamps_page_below_1_to_first_page()
    {
        for (var i = 1; i <= 3; i++)
        {
            await SeedStaffAsync($"x{i:D3}", fullName: $"User {i:D2}");
        }

        var result = await CreateHandler().Handle(new GetStaffsQuery(Page: -5), default);

        result.PageNumber.Should().Be(1);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Clamps_page_size_above_100_to_100()
    {
        await SeedStaffAsync("z01", fullName: "Only One");

        var result = await CreateHandler().Handle(new GetStaffsQuery(PageSize: 500), default);

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Maps_dto_fields_correctly()
    {
        var (user, staff) = await SeedStaffAsync(
            "m01",
            fullName: "Mapped Staff",
            role: UserRole.Incharge,
            position: "Center Incharge",
            department: "Operations",
            joinedDate: new DateOnly(2023, 7, 15),
            email: "mapped_staff@example.com",
            phone: "0955100001");

        var result = await CreateHandler().Handle(new GetStaffsQuery(), default);

        var dto = result.Items.Single();
        dto.StaffId.Should().Be(staff.Id);
        dto.UserAccountId.Should().Be(user.Id);
        dto.Username.Should().Be(user.Username);
        dto.FullName.Should().Be("Mapped Staff");
        dto.Email.Should().Be("mapped_staff@example.com");
        dto.Phone.Should().Be("0955100001");
        dto.Role.Should().Be(UserRole.Incharge);
        dto.IsActive.Should().BeTrue();
        dto.Position.Should().Be("Center Incharge");
        dto.Department.Should().Be("Operations");
        dto.JoinedDate.Should().Be(new DateOnly(2023, 7, 15));
    }
}
