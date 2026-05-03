using Coravel.Mailer.Mail.Interfaces;
using Coravel.Queuing.Interfaces;
using Microsoft.Extensions.Logging;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Batches.Notifications;
using ZealEducation.Infrastructure.Services.Email.Mailables;

namespace ZealEducation.Infrastructure.Services.Email;

internal sealed class BatchCandidateEnrolledNotificationService(
    IQueue queue,
    IMailer mailer,
    ILogger<BatchCandidateEnrolledNotificationService> logger) : IBatchCandidateEnrolledNotificationService
{
    public Task QueueAsync(BatchCandidateEnrolledEmailModel model, CancellationToken cancellationToken)
    {
        queue.QueueAsyncTask(async () =>
        {
            try
            {
                await mailer.SendAsync(new BatchCandidateEnrolledMailable(model));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send batch-candidate-enrolled email to {Email}", model.RecipientEmail);
            }
        });
        return Task.CompletedTask;
    }
}
