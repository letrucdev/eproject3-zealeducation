using ZealEducation.Application.Features.Staffs.Queries.GetStaffStatistics;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Queries.Staffs;

[Collection(DatabaseCollection.Name)]
public class GetStaffStatisticsQueryHandlerTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;

    public GetStaffStatisticsQueryHandlerTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private GetStaffStatisticsQueryHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new GetStaffStatisticsQueryHandler(
            new GenericRepository<Staff>(ctx),
            new GenericRepository<UserAccount>(ctx));
    }

    private static UserAccount NewUser(string suffix, UserRole role, bool isActive = true) => new()
    {
        Id = Guid.NewGuid(),
        Username = $"stat_{suffix}",
        PasswordHash = "hash",
        FullName = $"Stat User {suffix}",
        Email = $"stat_{suffix}@example.com",
        Phone = $"0977{suffix}",
        Dob = new DateOnly(1991, 3, 3),
        Gender = Gender.Male,
        Role = role,
        IsActive = isActive
    };

    private async Task SeedStaffAsync(string suffix, UserRole role, bool isActive = true)
    {
        var user = NewUser(suffix, role, isActive);
        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            UserAccountId = user.Id,
            Position = "Generic",
            Department = "Generic",
            JoinedDate = new DateOnly(2024, 1, 1)
        };
        await using var ctx = _fixture.CreateDbContext();
        ctx.UserAccounts.Add(user);
        await ctx.SaveChangesAsync();
        ctx.Staff.Add(staff);
        await ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task Returns_zero_values_when_no_staff_exists()
    {
        var dto = await CreateHandler().Handle(new GetStaffStatisticsQuery(), default);

        dto.Total.Should().Be(0);
        dto.Active.Should().Be(0);
        dto.Inactive.Should().Be(0);
        dto.Incharge.Should().Be(0);
        dto.Counselor.Should().Be(0);
        dto.AccountsStaff.Should().Be(0);
    }

    [Fact]
    public async Task Counts_total_active_inactive_correctly()
    {
        await SeedStaffAsync("a01", UserRole.Counselor, isActive: true);
        await SeedStaffAsync("a02", UserRole.Counselor, isActive: true);
        await SeedStaffAsync("a03", UserRole.Faculty, isActive: false);

        var dto = await CreateHandler().Handle(new GetStaffStatisticsQuery(), default);

        dto.Total.Should().Be(3);
        dto.Active.Should().Be(2);
        dto.Inactive.Should().Be(1);
    }

    [Fact]
    public async Task Buckets_roles_correctly()
    {
        await SeedStaffAsync("r01", UserRole.Incharge);
        await SeedStaffAsync("r02", UserRole.Incharge);
        await SeedStaffAsync("r03", UserRole.Counselor);
        await SeedStaffAsync("r04", UserRole.AccountsStaff);
        await SeedStaffAsync("r05", UserRole.AccountsStaff);
        await SeedStaffAsync("r06", UserRole.AccountsStaff);
        await SeedStaffAsync("r07", UserRole.Faculty);

        var dto = await CreateHandler().Handle(new GetStaffStatisticsQuery(), default);

        dto.Total.Should().Be(7);
        dto.Incharge.Should().Be(2);
        dto.Counselor.Should().Be(1);
        dto.AccountsStaff.Should().Be(3);
    }

    [Fact]
    public async Task Excludes_users_with_non_staff_role()
    {
        await SeedStaffAsync("e01", UserRole.Counselor);

        var orphanUser = NewUser("orphan", UserRole.Candidate);
        var orphanStaff = new Staff
        {
            Id = Guid.NewGuid(),
            UserAccountId = orphanUser.Id,
            Position = "X",
            Department = "X",
            JoinedDate = new DateOnly(2024, 1, 1)
        };
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.UserAccounts.Add(orphanUser);
            await ctx.SaveChangesAsync();
            ctx.Staff.Add(orphanStaff);
            await ctx.SaveChangesAsync();
        }

        var dto = await CreateHandler().Handle(new GetStaffStatisticsQuery(), default);

        dto.Total.Should().Be(1);
        dto.Counselor.Should().Be(1);
    }
}
