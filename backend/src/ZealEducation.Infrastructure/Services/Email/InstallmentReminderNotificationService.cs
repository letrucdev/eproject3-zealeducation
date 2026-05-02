using Coravel.Mailer.Mail.Interfaces;
using Coravel.Queuing.Interfaces;
using Microsoft.Extensions.Logging;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Candidates.Notifications;
using ZealEducation.Infrastructure.Services.Email.Mailables;

namespace ZealEducation.Infrastructure.Services.Email;

internal sealed class InstallmentReminderNotificationService(
    IQueue queue,
    IMailer mailer,
    ILogger<InstallmentReminderNotificationService> logger) : IInstallmentReminderNotificationService
{
    public Task QueueAsync(InstallmentReminderEmailModel model, CancellationToken cancellationToken)
    {
        queue.QueueAsyncTask(async () =>
        {
            try
            {
                await mailer.SendAsync(new InstallmentReminderMailable(model));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send installment reminder email to {Email}", model.RecipientEmail);
            }
        });
        return Task.CompletedTask;
    }
}
