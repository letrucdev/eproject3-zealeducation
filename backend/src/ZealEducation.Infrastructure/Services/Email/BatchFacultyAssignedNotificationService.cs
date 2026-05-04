using Coravel.Mailer.Mail.Interfaces;
using Coravel.Queuing.Interfaces;
using Microsoft.Extensions.Logging;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Batches.Notifications;
using ZealEducation.Infrastructure.Services.Email.Mailables;

namespace ZealEducation.Infrastructure.Services.Email;

internal sealed class BatchFacultyAssignedNotificationService(
    IQueue queue,
    IMailer mailer,
    ILogger<BatchFacultyAssignedNotificationService> logger) : IBatchFacultyAssignedNotificationService
{
    public Task QueueAsync(BatchFacultyAssignedEmailModel model, CancellationToken cancellationToken)
    {
        queue.QueueAsyncTask(async () =>
        {
            try
            {
                await mailer.SendAsync(new BatchFacultyAssignedMailable(model));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send batch-faculty-assigned email to {Email}", model.RecipientEmail);
            }
        });
        return Task.CompletedTask;
    }
}
