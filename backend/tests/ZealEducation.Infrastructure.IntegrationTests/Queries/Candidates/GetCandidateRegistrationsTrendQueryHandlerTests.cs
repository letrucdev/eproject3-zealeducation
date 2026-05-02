using ZealEducation.Application.Features.Candidates.Queries.GetCandidateRegistrationsTrend;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Queries.Candidates;

[Collection(DatabaseCollection.Name)]
public class GetCandidateRegistrationsTrendQueryHandlerTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;

    public GetCandidateRegistrationsTrendQueryHandlerTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private GetCandidateRegistrationsTrendQueryHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new GetCandidateRegistrationsTrendQueryHandler(new GenericRepository<Candidate>(ctx));
    }

    private static UserAccount NewUser(string suffix) => new()
    {
        Id = Guid.NewGuid(),
        Username = $"trend_{suffix}",
        PasswordHash = "hash",
        FullName = $"Trend User {suffix}",
        Email = $"trend_{suffix}@example.com",
        Phone = $"0933{suffix}",
        Dob = new DateOnly(1998, 5, 5),
        Gender = Gender.Male,
        Role = UserRole.Candidate,
        IsActive = true
    };

    private async Task SeedCandidateAsync(string suffix, string code, DateTime registeredAt, CandidateStatus status = CandidateStatus.Active)
    {
        var user = NewUser(suffix);
        var candidate = new Candidate
        {
            Id = Guid.NewGuid(),
            UserAccountId = user.Id,
            CandidateCode = code,
            Status = status,
            RegisteredAt = registeredAt
        };
        await using var ctx = _fixture.CreateDbContext();
        ctx.UserAccounts.Add(user);
        await ctx.SaveChangesAsync();
        ctx.Candidates.Add(candidate);
        await ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task Returns_zero_filled_buckets_when_no_candidates_exist()
    {
        var result = await CreateHandler().Handle(new GetCandidateRegistrationsTrendQuery(7), default);

        result.Should().HaveCount(7);
        result.Should().OnlyContain(p => p.Count == 0 && p.Graduated == 0 && p.Dropped == 0);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        result.Last().Date.Should().Be(today);
        result.First().Date.Should().Be(today.AddDays(-6));
    }

    [Fact]
    public async Task Counts_registrations_per_day_with_status_breakdown()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayMid = today.ToDateTime(new TimeOnly(12, 0));
        var twoDaysAgo = today.AddDays(-2).ToDateTime(new TimeOnly(9, 0));

        await SeedCandidateAsync("g01", "T0001", todayMid, CandidateStatus.Active);
        await SeedCandidateAsync("g02", "T0002", todayMid, CandidateStatus.Graduated);
        await SeedCandidateAsync("g03", "T0003", todayMid, CandidateStatus.Dropped);
        await SeedCandidateAsync("g04", "T0004", twoDaysAgo, CandidateStatus.Active);

        var result = await CreateHandler().Handle(new GetCandidateRegistrationsTrendQuery(7), default);

        result.Should().HaveCount(7);

        var todayBucket = result.Single(p => p.Date == today);
        todayBucket.Count.Should().Be(3);
        todayBucket.Graduated.Should().Be(1);
        todayBucket.Dropped.Should().Be(1);

        var twoDaysAgoBucket = result.Single(p => p.Date == today.AddDays(-2));
        twoDaysAgoBucket.Count.Should().Be(1);
        twoDaysAgoBucket.Graduated.Should().Be(0);
        twoDaysAgoBucket.Dropped.Should().Be(0);
    }

    [Fact]
    public async Task Excludes_registrations_outside_window()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayMid = today.ToDateTime(new TimeOnly(12, 0));
        var farPast = today.AddDays(-30).ToDateTime(new TimeOnly(8, 0));

        await SeedCandidateAsync("w01", "W0001", todayMid);
        await SeedCandidateAsync("w02", "W0002", farPast);

        var result = await CreateHandler().Handle(new GetCandidateRegistrationsTrendQuery(7), default);

        result.Sum(p => p.Count).Should().Be(1);
        result.Single(p => p.Date == today).Count.Should().Be(1);
    }

    [Fact]
    public async Task Returns_thirty_buckets_for_thirty_day_window()
    {
        var result = await CreateHandler().Handle(new GetCandidateRegistrationsTrendQuery(30), default);

        result.Should().HaveCount(30);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        result.First().Date.Should().Be(today.AddDays(-29));
        result.Last().Date.Should().Be(today);
    }

    [Fact]
    public async Task Returns_ninety_buckets_for_ninety_day_window()
    {
        var result = await CreateHandler().Handle(new GetCandidateRegistrationsTrendQuery(90), default);

        result.Should().HaveCount(90);
    }

    [Fact]
    public void Validator_only_allows_seven_thirty_or_ninety_days()
    {
        var validator = new GetCandidateRegistrationsTrendQueryValidator();

        validator.Validate(new GetCandidateRegistrationsTrendQuery(7)).IsValid.Should().BeTrue();
        validator.Validate(new GetCandidateRegistrationsTrendQuery(30)).IsValid.Should().BeTrue();
        validator.Validate(new GetCandidateRegistrationsTrendQuery(90)).IsValid.Should().BeTrue();
        validator.Validate(new GetCandidateRegistrationsTrendQuery(0)).IsValid.Should().BeFalse();
        validator.Validate(new GetCandidateRegistrationsTrendQuery(15)).IsValid.Should().BeFalse();
        validator.Validate(new GetCandidateRegistrationsTrendQuery(60)).IsValid.Should().BeFalse();
    }
}
