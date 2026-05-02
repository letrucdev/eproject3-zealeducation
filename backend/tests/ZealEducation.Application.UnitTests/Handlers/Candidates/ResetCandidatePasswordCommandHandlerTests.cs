using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Candidates.Commands.ResetCandidatePassword;
using ZealEducation.Application.Features.Candidates.Notifications;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Candidates;

public class ResetCandidatePasswordCommandHandlerTests
{
    private readonly Mock<IRepository<Candidate>> _candidateRepo = new();
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ICandidatePasswordResetNotificationService> _notification = new();
    private readonly ILogger<ResetCandidatePasswordCommandHandler> _logger = NullLogger<ResetCandidatePasswordCommandHandler>.Instance;

    private ResetCandidatePasswordCommandHandler CreateHandler() => new(
        _candidateRepo.Object,
        _userRepo.Object,
        _uow.Object,
        _hasher.Object,
        _notification.Object,
        _logger);

    [Fact]
    public async Task Throws_NotFoundException_when_candidate_does_not_exist()
    {
        var candidateId = Guid.NewGuid();
        _candidateRepo.SetupGetById(candidateId, null);

        var act = async () => await CreateHandler().Handle(new ResetCandidatePasswordCommand(candidateId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_user_account_does_not_exist()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var candidate = new Candidate { Id = candidateId, UserAccountId = userAccountId };
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, null);

        var act = async () => await CreateHandler().Handle(new ResetCandidatePasswordCommand(candidateId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_ConflictException_when_user_role_is_not_candidate()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var candidate = new Candidate { Id = candidateId, UserAccountId = userAccountId };
        var user = new UserAccount { Id = userAccountId, Role = UserRole.Counselor };
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, user);

        var act = async () => await CreateHandler().Handle(new ResetCandidatePasswordCommand(candidateId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("This user account does not belong to a candidate.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Resets_password_hashes_new_value_and_marks_must_change_password()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var candidate = new Candidate
        {
            Id = candidateId,
            UserAccountId = userAccountId,
            CandidateCode = "C-0001"
        };
        var user = new UserAccount
        {
            Id = userAccountId,
            Username = "alice",
            Email = "alice@example.com",
            FullName = "Alice",
            PasswordHash = "old-hash",
            Role = UserRole.Candidate,
            MustChangePassword = false,
            FailedLoginCount = 5
        };
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, user);

        string? capturedTempPassword = null;
        _hasher.Setup(h => h.Hash(It.IsAny<string>()))
            .Callback<string>(p => capturedTempPassword = p)
            .Returns<string>(p => $"hashed::{p}");

        var response = await CreateHandler().Handle(new ResetCandidatePasswordCommand(candidateId), default);

        capturedTempPassword.Should().NotBeNullOrEmpty();
        capturedTempPassword!.Length.Should().Be(12);

        user.PasswordHash.Should().Be($"hashed::{capturedTempPassword}");
        user.MustChangePassword.Should().BeTrue();
        user.FailedLoginCount.Should().Be(0);

        _hasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Once);
        _userRepo.Verify(r => r.Update(user), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        response.Username.Should().Be("alice");
        response.TemporaryPassword.Should().Be(capturedTempPassword);
    }

    [Fact]
    public async Task Queues_email_notification_when_user_has_email()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var candidate = new Candidate
        {
            Id = candidateId,
            UserAccountId = userAccountId,
            CandidateCode = "C-0042"
        };
        var user = new UserAccount
        {
            Id = userAccountId,
            Username = "bob",
            FullName = "Bob",
            Email = "bob@example.com",
            PasswordHash = "old",
            Role = UserRole.Candidate
        };
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, user);
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("new-hash");

        CandidatePasswordResetEmailModel? capturedModel = null;
        _notification
            .Setup(n => n.QueueAsync(It.IsAny<CandidatePasswordResetEmailModel>(), It.IsAny<CancellationToken>()))
            .Callback<CandidatePasswordResetEmailModel, CancellationToken>((m, _) => capturedModel = m)
            .Returns(Task.CompletedTask);

        var response = await CreateHandler().Handle(new ResetCandidatePasswordCommand(candidateId), default);

        capturedModel.Should().NotBeNull();
        capturedModel!.RecipientEmail.Should().Be("bob@example.com");
        capturedModel.RecipientName.Should().Be("Bob");
        capturedModel.Username.Should().Be("bob");
        capturedModel.CandidateCode.Should().Be("C-0042");
        capturedModel.TemporaryPassword.Should().Be(response.TemporaryPassword);
    }

    [Fact]
    public async Task Skips_email_notification_when_user_email_is_blank()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var candidate = new Candidate { Id = candidateId, UserAccountId = userAccountId, CandidateCode = "C-1" };
        var user = new UserAccount
        {
            Id = userAccountId,
            Username = "noemail",
            FullName = "No Email",
            Email = "",
            PasswordHash = "old",
            Role = UserRole.Candidate
        };
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, user);
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("new-hash");

        await CreateHandler().Handle(new ResetCandidatePasswordCommand(candidateId), default);

        _notification.Verify(
            n => n.QueueAsync(It.IsAny<CandidatePasswordResetEmailModel>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Swallows_notification_exceptions_and_still_returns_response()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var candidate = new Candidate { Id = candidateId, UserAccountId = userAccountId, CandidateCode = "C-1" };
        var user = new UserAccount
        {
            Id = userAccountId,
            Username = "carol",
            FullName = "Carol",
            Email = "carol@example.com",
            PasswordHash = "old",
            Role = UserRole.Candidate
        };
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, user);
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("new-hash");
        _notification
            .Setup(n => n.QueueAsync(It.IsAny<CandidatePasswordResetEmailModel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("smtp down"));

        var response = await CreateHandler().Handle(new ResetCandidatePasswordCommand(candidateId), default);

        response.Username.Should().Be("carol");
        response.TemporaryPassword.Should().NotBeNullOrEmpty();
    }
}
