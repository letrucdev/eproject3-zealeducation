using Coravel.Mailer.Mail.Interfaces;
using Coravel.Queuing.Interfaces;
using Microsoft.Extensions.Logging;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Candidates.Notifications;
using ZealEducation.Infrastructure.Services.Email.Mailables;

namespace ZealEducation.Infrastructure.Services.Email;

internal sealed class CourseAddedNotificationService(
    IQueue queue,
    IMailer mailer,
    ILogger<CourseAddedNotificationService> logger) : ICourseAddedNotificationService
{
    public Task QueueAsync(CourseAddedEmailModel model, CancellationToken cancellationToken)
    {
        queue.QueueAsyncTask(async () =>
        {
            try
            {
                await mailer.SendAsync(new CourseAddedMailable(model));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send course-added email to {Email}", model.RecipientEmail);
            }
        });
        return Task.CompletedTask;
    }
}
