using ZealEducation.Application.Features.Candidates.Notifications;

namespace ZealEducation.Application.Common.Interfaces;

public interface ICandidatePasswordResetNotificationService
{
    Task QueueAsync(CandidatePasswordResetEmailModel model, CancellationToken cancellationToken);
}
