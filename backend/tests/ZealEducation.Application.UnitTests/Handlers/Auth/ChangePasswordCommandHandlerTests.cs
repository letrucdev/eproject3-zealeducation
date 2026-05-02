using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Auth.Commands.ChangePassword;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Auth;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private ChangePasswordCommandHandler CreateHandler() => new(
        _userRepo.Object, _uow.Object, _hasher.Object, _currentUser.Object);

    private static UserAccount BuildUser(Guid id, string passwordHash = "$old-hash$", bool mustChange = true) => new()
    {
        Id = id,
        Username = "alice",
        PasswordHash = passwordHash,
        FullName = "Alice Doe",
        Email = "alice@example.com",
        Phone = "0900000000",
        Dob = new DateOnly(1990, 1, 1),
        Gender = Gender.Female,
        Role = UserRole.Counselor,
        IsActive = true,
        MustChangePassword = mustChange
    };

    [Fact]
    public async Task Throws_unauthorized_when_current_user_is_not_resolved()
    {
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(
            new ChangePasswordCommand("Old1!", "New12345!"), default);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Not authenticated.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_unauthorized_when_user_not_found()
    {
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _userRepo.SetupGetById(userId, null);

        var act = async () => await CreateHandler().Handle(
            new ChangePasswordCommand("Old1!", "New12345!"), default);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("User not found.");
    }

    [Fact]
    public async Task Throws_unauthorized_when_current_password_is_incorrect()
    {
        var userId = Guid.NewGuid();
        var user = BuildUser(userId);
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _userRepo.SetupGetById(userId, user);
        _hasher.Setup(h => h.Verify("WrongOld!", user.PasswordHash)).Returns(false);

        var act = async () => await CreateHandler().Handle(
            new ChangePasswordCommand("WrongOld!", "New12345!"), default);

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("Current password is incorrect.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Hashes_new_password_clears_must_change_flag_and_persists_changes()
    {
        var userId = Guid.NewGuid();
        var user = BuildUser(userId, mustChange: true);
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _userRepo.SetupGetById(userId, user);
        _hasher.Setup(h => h.Verify("OldPass1!", user.PasswordHash)).Returns(true);
        _hasher.Setup(h => h.Hash("NewPass2!")).Returns("$new-hash$");

        await CreateHandler().Handle(new ChangePasswordCommand("OldPass1!", "NewPass2!"), default);

        user.PasswordHash.Should().Be("$new-hash$");
        user.MustChangePassword.Should().BeFalse();
        _userRepo.Verify(r => r.Update(user), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
