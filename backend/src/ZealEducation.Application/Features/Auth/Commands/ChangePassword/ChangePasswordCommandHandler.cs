using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    IRepository<UserAccount> userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ICurrentUser currentUser) : IRequestHandler<ChangePasswordCommand, Unit>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedException("Not authenticated.");

        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedException("User not found.");

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedException("Current password is incorrect.");

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.MustChangePassword = false;
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
