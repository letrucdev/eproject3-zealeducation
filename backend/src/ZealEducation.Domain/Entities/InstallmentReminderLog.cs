using ZealEducation.Domain.Common;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Domain.Entities;

public class InstallmentReminderLog : BaseAuditableEntity
{
    public Guid InstallmentPlanId { get; set; }
    public ReminderType ReminderType { get; set; }
    public int DaysOffset { get; set; }
    public DateOnly SentForDate { get; set; }
    public string RecipientEmail { get; set; } = default!;

    public InstallmentPlan InstallmentPlan { get; set; } = default!;
}
