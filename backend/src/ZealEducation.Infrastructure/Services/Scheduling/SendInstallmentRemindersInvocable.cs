using Coravel.Invocable;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Candidates.Notifications;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.Data;

namespace ZealEducation.Infrastructure.Services.Scheduling;

public sealed class SendInstallmentRemindersInvocable(
    ApplicationDbContext db,
    IInstallmentReminderNotificationService notifier,
    ILogger<SendInstallmentRemindersInvocable> logger) : IInvocable
{
    private const int OverdueCadenceDays = 5;
    private const int OverdueMaxDays = 30;
    private const int BeforeDueOffsetDays = 3;

    public async Task Invoke()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        logger.LogInformation("Running installment reminder sweep for {Date}", today);

        var openInstallments = await db.InstallmentPlans
            .Where(p => p.Status != InstallmentStatus.Paid)
            .Where(p => p.FeeStructure.PaymentStatus != PaymentStatus.Paid)
            .Where(p => p.FeeStructure.Candidate.Status == CandidateStatus.Active)
            .Include(p => p.FeeStructure)
                .ThenInclude(f => f.Candidate)
                    .ThenInclude(c => c.UserAccount)
            .ToListAsync();

        var alreadySent = await db.Set<InstallmentReminderLog>()
            .Where(l => l.SentForDate == today)
            .Select(l => new { l.InstallmentPlanId, l.ReminderType })
            .ToListAsync();
        var sentSet = alreadySent
            .Select(x => (x.InstallmentPlanId, x.ReminderType))
            .ToHashSet();

        var queued = 0;
        foreach (var plan in openInstallments)
        {
            var (type, offset) = ClassifyReminder(plan.DueDate, today);
            if (type is null) continue;
            if (sentSet.Contains((plan.Id, type.Value))) continue;

            var account = plan.FeeStructure.Candidate.UserAccount;

            db.Set<InstallmentReminderLog>().Add(new InstallmentReminderLog
            {
                Id = Guid.NewGuid(),
                InstallmentPlanId = plan.Id,
                ReminderType = type.Value,
                DaysOffset = offset,
                SentForDate = today,
                RecipientEmail = account.Email,
            });

            await notifier.QueueAsync(new InstallmentReminderEmailModel
            {
                RecipientEmail = account.Email,
                RecipientName = account.FullName,
                CandidateCode = plan.FeeStructure.Candidate.CandidateCode,
                InstallmentNo = plan.InstallmentNo,
                AmountDue = plan.AmountDue,
                AmountPaid = plan.AmountPaid,
                Outstanding = plan.AmountDue - plan.AmountPaid,
                DueDate = plan.DueDate,
                DaysOffset = offset,
                ReminderType = type.Value,
            }, CancellationToken.None);

            queued++;
        }

        try
        {
            await db.SaveChangesAsync();
            logger.LogInformation("Queued {Count} installment reminder emails", queued);
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Some installment reminder log rows conflicted (likely concurrent run)");
        }
    }

    private static (ReminderType? type, int offset) ClassifyReminder(DateOnly dueDate, DateOnly today)
    {
        var diff = dueDate.DayNumber - today.DayNumber;

        if (diff == BeforeDueOffsetDays)
        {
            return (ReminderType.BeforeDue, -BeforeDueOffsetDays);
        }

        if (diff < 0)
        {
            var overdueDays = -diff;
            if (overdueDays <= OverdueMaxDays && overdueDays % OverdueCadenceDays == 0)
            {
                return (ReminderType.Overdue, overdueDays);
            }
        }

        return (null, 0);
    }
}
