using System.Security.Cryptography;
using MediatR;
using Microsoft.Extensions.Logging;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Candidates.Notifications;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Candidates.Commands.ResetCandidatePassword;

public class ResetCandidatePasswordCommandHandler(
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ICandidatePasswordResetNotificationService notificationService,
    ILogger<ResetCandidatePasswordCommandHandler> logger) : IRequestHandler<ResetCandidatePasswordCommand, ResetCandidatePasswordResponse>
{
    private const string PasswordAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%";

    public async Task<ResetCandidatePasswordResponse> Handle(ResetCandidatePasswordCommand request, CancellationToken cancellationToken)
    {
        var candidate = await candidateRepository.GetByIdAsync(request.CandidateId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidate), request.CandidateId);

        var user = await userRepository.GetByIdAsync(candidate.UserAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAccount), candidate.UserAccountId);

        if (user.Role != UserRole.Candidate)
            throw new ConflictException("This user account does not belong to a candidate.");

        var tempPassword = GenerateRandomString(12, PasswordAlphabet);
        var resetAt = DateTime.UtcNow;

        user.PasswordHash = passwordHasher.Hash(tempPassword);
        user.MustChangePassword = true;
        user.FailedLoginCount = 0;
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await TryQueuePasswordResetEmailAsync(user, candidate, tempPassword, resetAt, cancellationToken);

        return new ResetCandidatePasswordResponse
        {
            Username = user.Username,
            TemporaryPassword = tempPassword
        };
    }

    private async Task TryQueuePasswordResetEmailAsync(
        UserAccount user,
        Candidate candidate,
        string tempPassword,
        DateTime resetAt,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
            return;

        try
        {
            var model = new CandidatePasswordResetEmailModel
            {
                RecipientEmail = user.Email,
                RecipientName = user.FullName,
                Username = user.Username,
                TemporaryPassword = tempPassword,
                CandidateCode = candidate.CandidateCode,
                ResetAt = resetAt
            };

            await notificationService.QueueAsync(model, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to queue password-reset email for candidate {CandidateId}", candidate.Id);
        }
    }

    private static string GenerateRandomString(int length, string alphabet)
    {
        var buffer = new char[length];
        for (var i = 0; i < length; i++)
        {
            var idx = RandomNumberGenerator.GetInt32(alphabet.Length);
            buffer[i] = alphabet[idx];
        }
        return new string(buffer);
    }
}
