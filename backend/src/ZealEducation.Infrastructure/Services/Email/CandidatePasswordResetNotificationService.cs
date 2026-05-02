using Coravel.Mailer.Mail.Interfaces;
using Coravel.Queuing.Interfaces;
using Microsoft.Extensions.Logging;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Candidates.Notifications;
using ZealEducation.Infrastructure.Services.Email.Mailables;

namespace ZealEducation.Infrastructure.Services.Email;

internal sealed class CandidatePasswordResetNotificationService(
    IQueue queue,
    IMailer mailer,
    ILogger<CandidatePasswordResetNotificationService> logger) : ICandidatePasswordResetNotificationService
{
    public Task QueueAsync(CandidatePasswordResetEmailModel model, CancellationToken cancellationToken)
    {
        queue.QueueAsyncTask(async () =>
        {
            try
            {
                await mailer.SendAsync(new CandidatePasswordResetMailable(model));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send candidate password-reset email to {Email}", model.RecipientEmail);
            }
        });
        return Task.CompletedTask;
    }
}
