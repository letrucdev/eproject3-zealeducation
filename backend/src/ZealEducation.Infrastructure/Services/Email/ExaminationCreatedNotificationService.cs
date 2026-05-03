using Coravel.Mailer.Mail.Interfaces;
using Coravel.Queuing.Interfaces;
using Microsoft.Extensions.Logging;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Examinations.Notifications;
using ZealEducation.Infrastructure.Services.Email.Mailables;

namespace ZealEducation.Infrastructure.Services.Email;

internal sealed class ExaminationCreatedNotificationService(
    IQueue queue,
    IMailer mailer,
    ILogger<ExaminationCreatedNotificationService> logger) : IExaminationCreatedNotificationService
{
    public Task QueueAsync(ExaminationCreatedEmailModel model, CancellationToken cancellationToken)
    {
        queue.QueueAsyncTask(async () =>
        {
            try
            {
                await mailer.SendAsync(new ExaminationCreatedMailable(model));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send examination-created email to {Email}", model.RecipientEmail);
            }
        });
        return Task.CompletedTask;
    }
}
