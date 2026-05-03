using Microsoft.Extensions.Options;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Auth.Commands.Login;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.IntegrationTests.Helpers;
using ZealEducation.Infrastructure.Repositories;
using ZealEducation.Infrastructure.Services;

namespace ZealEducation.Infrastructure.IntegrationTests.Handlers.Auth;

[Collection(DatabaseCollection.Name)]
public class LoginCommandHandlerTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;
    private readonly BCryptPasswordHasher _passwordHasher = new();
    private readonly JwtTokenService _jwtTokenService = new(Options.Create(new JwtSettings
    {
        Issuer = "zeal-test",
        Audience = "zeal-test",
        SecretKey = "test-secret-key-very-long-for-hmac-sha256-zeal-education",
        ExpiryMinutes = 60
    }));

    public LoginCommandHandlerTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private LoginCommandHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new LoginCommandHandler(
            new GenericRepository<Domain.Entities.UserAccount>(ctx),
            new GenericRepository<Domain.Entities.Staff>(ctx),
            new GenericRepository<Domain.Entities.Faculty>(ctx),
            new GenericRepository<Domain.Entities.Candidate>(ctx),
            ctx,
            _passwordHasher,
            _jwtTokenService);
    }

    [Fact]
    public async Task Should_throw_when_user_does_not_exist()
    {
        var act = async () => await CreateHandler().Handle(new LoginCommand("ghost", "pwd"), default);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task Should_throw_when_user_is_inactive()
    {
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.UserAccounts.Add(EntityBuilders.NewUser(
                username: "inactive.user",
                passwordHash: _passwordHasher.Hash("Pass123!"),
                isActive: false));
            await ctx.SaveChangesAsync();
        }

        var act = async () => await CreateHandler().Handle(new LoginCommand("inactive.user", "Pass123!"), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Should_increment_failed_count_when_password_wrong()
    {
        var hash = _passwordHasher.Hash("Correct123!");
        var userId = Guid.Empty;
        await using (var ctx = _fixture.CreateDbContext())
        {
            var user = EntityBuilders.NewUser(username: "alice", passwordHash: hash);
            ctx.UserAccounts.Add(user);
            await ctx.SaveChangesAsync();
            userId = user.Id;
        }

        var act = async () => await CreateHandler().Handle(new LoginCommand("alice", "Wrong!"), default);

        await act.Should().ThrowAsync<UnauthorizedException>();

        await using (var ctx = _fixture.CreateDbContext())
        {
            var u = await ctx.UserAccounts.FindAsync(userId);
            u!.FailedLoginCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task Should_return_token_and_reset_failed_count_on_successful_login()
    {
        var hash = _passwordHasher.Hash("Correct123!");
        var userId = Guid.Empty;
        await using (var ctx = _fixture.CreateDbContext())
        {
            var user = EntityBuilders.NewUser(
                username: "bob",
                passwordHash: hash,
                role: UserRole.Counselor,
                failedLoginCount: 2);
            ctx.UserAccounts.Add(user);
            await ctx.SaveChangesAsync();
            userId = user.Id;
        }

        var response = await CreateHandler().Handle(new LoginCommand("bob", "Correct123!"), default);

        response.Token.Should().NotBeNullOrEmpty();
        response.User.Username.Should().Be("bob");
        response.User.Role.Should().Be(UserRole.Counselor);

        await using (var ctx = _fixture.CreateDbContext())
        {
            var u = await ctx.UserAccounts.FindAsync(userId);
            u!.FailedLoginCount.Should().Be(0);
            u.LastLogin.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Should_populate_candidate_info_for_candidate_login()
    {
        var hash = _passwordHasher.Hash("Pass123!");
        await using (var ctx = _fixture.CreateDbContext())
        {
            var user = EntityBuilders.NewUser(
                username: "candidate1",
                passwordHash: hash,
                role: UserRole.Candidate);
            ctx.UserAccounts.Add(user);
            ctx.Candidates.Add(EntityBuilders.NewCandidate(user.Id, code: "CAN-100"));
            await ctx.SaveChangesAsync();
        }

        var response = await CreateHandler().Handle(new LoginCommand("candidate1", "Pass123!"), default);

        response.User.Candidate.Should().NotBeNull();
        response.User.Candidate!.CandidateCode.Should().Be("CAN-100");
    }

    [Fact]
    public async Task Should_return_password_reset_token_when_must_change_password()
    {
        var hash = _passwordHasher.Hash("Temp1234!");
        await using (var ctx = _fixture.CreateDbContext())
        {
            var user = EntityBuilders.NewUser(
                username: "newuser",
                passwordHash: hash,
                role: UserRole.Counselor,
                mustChangePassword: true);
            ctx.UserAccounts.Add(user);
            await ctx.SaveChangesAsync();
        }

        var response = await CreateHandler().Handle(new LoginCommand("newuser", "Temp1234!"), default);

        response.Token.Should().NotBeNullOrEmpty();
        response.User.MustChangePassword.Should().BeTrue();
        // Reset token has shorter expiry (~5 min) per JwtTokenService.PasswordResetTokenExpiryMinutes
        (response.ExpiresAt - DateTime.UtcNow).Should().BeLessThan(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task Should_populate_staff_info_for_staff_role_login()
    {
        var hash = _passwordHasher.Hash("Pass123!");
        await using (var ctx = _fixture.CreateDbContext())
        {
            var user = EntityBuilders.NewUser(
                username: "counselor1",
                passwordHash: hash,
                role: UserRole.Counselor);
            ctx.UserAccounts.Add(user);
            ctx.Staff.Add(EntityBuilders.NewStaff(user.Id));
            await ctx.SaveChangesAsync();
        }

        var response = await CreateHandler().Handle(new LoginCommand("counselor1", "Pass123!"), default);

        response.User.Staff.Should().NotBeNull();
        response.User.Staff!.Position.Should().Be("Counselor");
        response.User.Faculty.Should().BeNull();
    }

    [Fact]
    public async Task Should_populate_staff_and_faculty_info_for_faculty_role_login()
    {
        var hash = _passwordHasher.Hash("Pass123!");
        await using (var ctx = _fixture.CreateDbContext())
        {
            var user = EntityBuilders.NewUser(
                username: "prof1",
                passwordHash: hash,
                role: UserRole.Faculty);
            var staff = EntityBuilders.NewStaff(user.Id);
            ctx.UserAccounts.Add(user);
            ctx.Staff.Add(staff);
            ctx.Faculties.Add(EntityBuilders.NewFaculty(staff.Id, code: "F-100"));
            await ctx.SaveChangesAsync();
        }

        var response = await CreateHandler().Handle(new LoginCommand("prof1", "Pass123!"), default);

        response.User.Staff.Should().NotBeNull();
        response.User.Faculty.Should().NotBeNull();
        response.User.Faculty!.FacultyCode.Should().Be("F-100");
    }
}
