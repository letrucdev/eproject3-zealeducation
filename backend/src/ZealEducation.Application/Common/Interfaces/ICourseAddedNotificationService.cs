using ZealEducation.Application.Features.Candidates.Notifications;

namespace ZealEducation.Application.Common.Interfaces;

public interface ICourseAddedNotificationService
{
    Task QueueAsync(CourseAddedEmailModel model, CancellationToken cancellationToken);
}
