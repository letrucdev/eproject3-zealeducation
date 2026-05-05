using ZealEducation.Application.Features.Batches.Notifications;

namespace ZealEducation.Application.Common.Interfaces;

public interface IBatchFacultyAssignedNotificationService
{
    Task QueueAsync(BatchFacultyAssignedEmailModel model, CancellationToken cancellationToken);
}
