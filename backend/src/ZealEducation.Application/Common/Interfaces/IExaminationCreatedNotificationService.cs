using ZealEducation.Application.Features.Examinations.Notifications;

namespace ZealEducation.Application.Common.Interfaces;

public interface IExaminationCreatedNotificationService
{
    Task QueueAsync(ExaminationCreatedEmailModel model, CancellationToken cancellationToken);
}
