using ZealEducation.Application.Features.Batches.Notifications;

namespace ZealEducation.Application.Common.Interfaces;

public interface IBatchCandidateEnrolledNotificationService
{
    Task QueueAsync(BatchCandidateEnrolledEmailModel model, CancellationToken cancellationToken);
}
