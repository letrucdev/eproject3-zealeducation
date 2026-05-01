using ZealEducation.Application.Features.CourseEnquiries.Notifications;

namespace ZealEducation.Application.Common.Interfaces;

public interface IEnquiryConvertedNotificationService
{
    Task QueueAsync(EnquiryConvertedEmailModel model, CancellationToken cancellationToken);
}
