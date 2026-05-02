using ZealEducation.Application.Features.Candidates.Queries.GetCandidates;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Queries.Candidates;

[Collection(DatabaseCollection.Name)]
public class GetCandidatesQueryHandlerTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;

    public GetCandidatesQueryHandlerTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private GetCandidatesQueryHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new GetCandidatesQueryHandler(
            new GenericRepository<Candidate>(ctx),
            new GenericRepository<UserAccount>(ctx),
            new GenericRepository<Enrollment>(ctx),
            new GenericRepository<Course>(ctx),
            new GenericRepository<Batch>(ctx));
    }

    private static UserAccount NewUser(
        string suffix,
        string fullName = "Candidate User",
        string? email = null,
        string? phone = null,
        bool isActive = true) => new()
        {
            Id = Guid.NewGuid(),
            Username = $"cand_{suffix}",
            PasswordHash = "hash",
            FullName = fullName,
            Email = email ?? $"cand_{suffix}@example.com",
            Phone = phone ?? $"0900{suffix}",
            Dob = new DateOnly(2000, 1, 1),
            Gender = Gender.Male,
            Role = UserRole.Candidate,
            IsActive = isActive
        };

    private static Candidate NewCandidate(
        Guid userAccountId,
        string code,
        CandidateStatus status = CandidateStatus.Active,
        DateTime? registeredAt = null) => new()
        {
            Id = Guid.NewGuid(),
            UserAccountId = userAccountId,
            CandidateCode = code,
            Status = status,
            RegisteredAt = registeredAt ?? DateTime.UtcNow
        };

    private async Task<(UserAccount user, Candidate candidate)> SeedCandidateAsync(
        string suffix,
        string code,
        string fullName = "Candidate User",
        string? email = null,
        string? phone = null,
        CandidateStatus status = CandidateStatus.Active,
        bool isActive = true,
        DateTime? registeredAt = null)
    {
        var user = NewUser(suffix, fullName, email, phone, isActive);
        var candidate = NewCandidate(user.Id, code, status, registeredAt);
        await using var ctx = _fixture.CreateDbContext();
        ctx.UserAccounts.Add(user);
        await ctx.SaveChangesAsync();
        ctx.Candidates.Add(candidate);
        await ctx.SaveChangesAsync();
        return (user, candidate);
    }

    [Fact]
    public async Task Returns_empty_list_when_no_candidates_exist()
    {
        var result = await CreateHandler().Handle(new GetCandidatesQuery(), default);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Returns_paginated_candidates_with_default_paging()
    {
        for (var i = 1; i <= 12; i++)
        {
            await SeedCandidateAsync($"{i:D3}", $"C{i:D4}", fullName: $"User {i:D2}");
        }

        var result = await CreateHandler().Handle(new GetCandidatesQuery(), default);

        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(12);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task Filters_by_status()
    {
        await SeedCandidateAsync("a01", "C0001", status: CandidateStatus.Active);
        await SeedCandidateAsync("a02", "C0002", status: CandidateStatus.Graduated);
        await SeedCandidateAsync("a03", "C0003", status: CandidateStatus.Dropped);

        var result = await CreateHandler().Handle(
            new GetCandidatesQuery(Status: CandidateStatus.Graduated), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().CandidateCode.Should().Be("C0002");
    }

    [Fact]
    public async Task Filters_by_search_in_full_name_case_insensitive()
    {
        await SeedCandidateAsync("s01", "C0001", fullName: "Alice Walker");
        await SeedCandidateAsync("s02", "C0002", fullName: "Bob Smith");
        await SeedCandidateAsync("s03", "C0003", fullName: "Charlotte Brown");

        var result = await CreateHandler().Handle(new GetCandidatesQuery(Search: "ALICE"), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().FullName.Should().Be("Alice Walker");
    }

    [Fact]
    public async Task Filters_by_search_in_candidate_code()
    {
        await SeedCandidateAsync("c01", "ABC123", fullName: "User One");
        await SeedCandidateAsync("c02", "XYZ999", fullName: "User Two");

        var result = await CreateHandler().Handle(new GetCandidatesQuery(Search: "abc"), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().CandidateCode.Should().Be("ABC123");
    }

    [Fact]
    public async Task Sorts_by_full_name_ascending()
    {
        await SeedCandidateAsync("o01", "C0001", fullName: "Charlie");
        await SeedCandidateAsync("o02", "C0002", fullName: "Alpha");
        await SeedCandidateAsync("o03", "C0003", fullName: "Bravo");

        var result = await CreateHandler().Handle(
            new GetCandidatesQuery(SortBy: "fullName", SortDirection: "asc"), default);

        result.Items.Select(i => i.FullName).Should().ContainInOrder("Alpha", "Bravo", "Charlie");
    }

    [Fact]
    public async Task Sorts_by_candidate_code_descending()
    {
        await SeedCandidateAsync("k01", "C0001", fullName: "U1");
        await SeedCandidateAsync("k02", "C0002", fullName: "U2");
        await SeedCandidateAsync("k03", "C0003", fullName: "U3");

        var result = await CreateHandler().Handle(
            new GetCandidatesQuery(SortBy: "candidateCode", SortDirection: "desc"), default);

        result.Items.Select(i => i.CandidateCode).Should().ContainInOrder("C0003", "C0002", "C0001");
    }

    [Fact]
    public async Task Defaults_to_registered_at_descending_when_sort_unspecified()
    {
        var now = DateTime.UtcNow;
        await SeedCandidateAsync("d01", "C0001", fullName: "First", registeredAt: now.AddDays(-3));
        await SeedCandidateAsync("d02", "C0002", fullName: "Second", registeredAt: now.AddDays(-2));
        await SeedCandidateAsync("d03", "C0003", fullName: "Third", registeredAt: now.AddDays(-1));

        var result = await CreateHandler().Handle(new GetCandidatesQuery(), default);

        result.Items.Select(i => i.CandidateCode).Should().ContainInOrder("C0003", "C0002", "C0001");
    }

    [Fact]
    public async Task Returns_second_page_correctly()
    {
        for (var i = 1; i <= 12; i++)
        {
            await SeedCandidateAsync($"p{i:D3}", $"C{i:D4}", fullName: $"User {i:D2}");
        }

        var result = await CreateHandler().Handle(new GetCandidatesQuery(Page: 2, PageSize: 10), default);

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
            await SeedCandidateAsync($"x{i:D3}", $"C{i:D4}", fullName: $"User {i:D2}");
        }

        var result = await CreateHandler().Handle(new GetCandidatesQuery(Page: -1), default);

        result.PageNumber.Should().Be(1);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Clamps_page_size_above_100_to_100()
    {
        await SeedCandidateAsync("z01", "C0001");

        var result = await CreateHandler().Handle(new GetCandidatesQuery(PageSize: 500), default);

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Maps_dto_fields_correctly()
    {
        var (user, candidate) = await SeedCandidateAsync(
            "m01", "MAP001",
            fullName: "Mapped User",
            email: "mapped@example.com",
            phone: "0911000001",
            status: CandidateStatus.Active);

        var result = await CreateHandler().Handle(new GetCandidatesQuery(), default);

        var dto = result.Items.Single();
        dto.CandidateId.Should().Be(candidate.Id);
        dto.CandidateCode.Should().Be("MAP001");
        dto.FullName.Should().Be("Mapped User");
        dto.Email.Should().Be("mapped@example.com");
        dto.Phone.Should().Be("0911000001");
        dto.IsActive.Should().BeTrue();
        dto.Status.Should().Be(CandidateStatus.Active);
        dto.CurrentEnrollmentId.Should().BeNull();
        dto.CurrentCourseId.Should().BeNull();
    }
}
