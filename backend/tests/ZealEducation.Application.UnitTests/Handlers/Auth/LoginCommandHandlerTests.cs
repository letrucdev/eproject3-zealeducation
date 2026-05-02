using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Auth.Commands.Login;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<IRepository<Faculty>> _facultyRepo = new();
    private readonly Mock<IRepository<Candidate>> _candidateRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenService> _jwt = new();

    private LoginCommandHandler CreateHandler() => new(
        _userRepo.Object,
        _staffRepo.Object,
        _facultyRepo.Object,
        _candidateRepo.Object,
        _uow.Object,
        _hasher.Object,
        _jwt.Object);

    private static UserAccount BuildUser(bool active = true, bool mustChange = false, UserRole role = UserRole.Counselor) => new()
    {
        Id = Guid.NewGuid(),
        Username = "alice",
        PasswordHash = "$hash$",
        FullName = "Alice Doe",
        Email = "alice@example.com",
        Phone = "0900000000",
        Dob = new DateOnly(1990, 1, 1),
        Gender = Gender.Female,
        Role = role,
        IsActive = active,
        MustChangePassword = mustChange
    };

    [Fact]
    public async Task Throws_unauthorized_when_user_is_inactive()
    {
        var user = BuildUser(active: false);
        _userRepo.SetupFind(new[] { user });

        var act = async () => await CreateHandler().Handle(new LoginCommand("alice", "pwd"), default);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task Throws_unauthorized_and_increments_failed_count_when_password_is_invalid()
    {
        var user = BuildUser();
        _userRepo.SetupFind(new[] { user });
        _hasher.Setup(h => h.Verify("badpwd", user.PasswordHash)).Returns(false);

        var act = async () => await CreateHandler().Handle(new LoginCommand("alice", "badpwd"), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
        user.FailedLoginCount.Should().Be(1);
        _userRepo.Verify(r => r.Update(user), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Returns_token_and_resets_failed_count_on_successful_login()
    {
        var user = BuildUser(role: UserRole.SystemAdmin);
        user.FailedLoginCount = 3;
        _userRepo.SetupFind(new[] { user });
        _hasher.Setup(h => h.Verify("pwd", user.PasswordHash)).Returns(true);
        _jwt.Setup(j => j.GenerateToken(user))
            .Returns(new JwtTokenResult("the-jwt", DateTime.UtcNow.AddHours(1)));
        _staffRepo.SetupFind(Array.Empty<Staff>());

        var response = await CreateHandler().Handle(new LoginCommand("alice", "pwd"), default);

        response.Token.Should().Be("the-jwt");
        response.User.Username.Should().Be("alice");
        response.User.Role.Should().Be(UserRole.SystemAdmin);
        user.FailedLoginCount.Should().Be(0);
        user.LastLogin.Should().NotBeNull();
        _jwt.Verify(j => j.GenerateToken(user), Times.Once);
        _jwt.Verify(j => j.GeneratePasswordResetToken(It.IsAny<UserAccount>()), Times.Never);
    }

    [Fact]
    public async Task Returns_password_reset_token_when_user_must_change_password()
    {
        var user = BuildUser(mustChange: true);
        _userRepo.SetupFind(new[] { user });
        _hasher.Setup(h => h.Verify("pwd", user.PasswordHash)).Returns(true);
        _jwt.Setup(j => j.GeneratePasswordResetToken(user))
            .Returns(new JwtTokenResult("reset-token", DateTime.UtcNow.AddMinutes(15)));

        var response = await CreateHandler().Handle(new LoginCommand("alice", "pwd"), default);

        response.Token.Should().Be("reset-token");
        response.User.MustChangePassword.Should().BeTrue();
        _jwt.Verify(j => j.GenerateToken(It.IsAny<UserAccount>()), Times.Never);
        _jwt.Verify(j => j.GeneratePasswordResetToken(user), Times.Once);
    }

    [Fact]
    public async Task Populates_candidate_info_when_role_is_candidate()
    {
        var user = BuildUser(role: UserRole.Candidate);
        var candidate = new Candidate
        {
            Id = Guid.NewGuid(),
            UserAccountId = user.Id,
            CandidateCode = "C-0001",
            Status = CandidateStatus.Active,
            RegisteredAt = DateTime.UtcNow
        };
        _userRepo.SetupFind(new[] { user });
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwt.Setup(j => j.GenerateToken(user)).Returns(new JwtTokenResult("t", DateTime.UtcNow.AddHours(1)));
        _candidateRepo.SetupFind(new[] { candidate });

        var response = await CreateHandler().Handle(new LoginCommand("alice", "pwd"), default);

        response.User.Candidate.Should().NotBeNull();
        response.User.Candidate!.CandidateCode.Should().Be("C-0001");
    }
}
