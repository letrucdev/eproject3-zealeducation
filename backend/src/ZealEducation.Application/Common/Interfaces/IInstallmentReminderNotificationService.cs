using ZealEducation.Application.Features.Candidates.Notifications;

namespace ZealEducation.Application.Common.Interfaces;

public interface IInstallmentReminderNotificationService
{
    Task QueueAsync(InstallmentReminderEmailModel model, CancellationToken cancellationToken);
}
