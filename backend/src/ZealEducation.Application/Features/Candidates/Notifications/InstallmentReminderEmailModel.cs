using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Candidates.Notifications;

public class InstallmentReminderEmailModel
{
    public string RecipientEmail { get; init; } = default!;
    public string RecipientName { get; init; } = default!;

    public string CandidateCode { get; init; } = default!;
    public int InstallmentNo { get; init; }
    public decimal AmountDue { get; init; }
    public decimal AmountPaid { get; init; }
    public decimal Outstanding { get; init; }
    public DateOnly DueDate { get; init; }
    public int DaysOffset { get; init; }
    public ReminderType ReminderType { get; init; }
}
