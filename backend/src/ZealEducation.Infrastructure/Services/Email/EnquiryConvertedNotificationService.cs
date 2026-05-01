using Coravel.Mailer.Mail.Interfaces;
using Coravel.Queuing.Interfaces;
using Microsoft.Extensions.Logging;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CourseEnquiries.Notifications;
using ZealEducation.Infrastructure.Services.Email.Mailables;

namespace ZealEducation.Infrastructure.Services.Email;

internal sealed class EnquiryConvertedNotificationService(
    IQueue queue,
    IMailer mailer,
    ILogger<EnquiryConvertedNotificationService> logger) : IEnquiryConvertedNotificationService
{
    public Task QueueAsync(EnquiryConvertedEmailModel model, CancellationToken cancellationToken)
    {
        queue.QueueAsyncTask(async () =>
        {
            try
            {
                await mailer.SendAsync(new EnquiryConvertedMailable(model));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send enquiry-converted email to {Email}", model.RecipientEmail);
            }
        });
        return Task.CompletedTask;
    }
}
