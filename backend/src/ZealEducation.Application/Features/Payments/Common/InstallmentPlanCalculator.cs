using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Payments.Common;

public static class InstallmentPlanCalculator
{
    public const int MinWeeksMonthly = 8;
    public const int MinWeeksQuarterly = 24;
    public const int MinWeeksBiYearly = 48;

    public static bool IsFrequencyAllowed(InstallmentFrequency frequency, int durationWeeks) => frequency switch
    {
        InstallmentFrequency.Monthly => durationWeeks >= MinWeeksMonthly,
        InstallmentFrequency.Quarterly => durationWeeks >= MinWeeksQuarterly,
        InstallmentFrequency.BiYearly => durationWeeks >= MinWeeksBiYearly,
        _ => false,
    };

    public static int IntervalMonths(InstallmentFrequency frequency) => frequency switch
    {
        InstallmentFrequency.Monthly => 1,
        InstallmentFrequency.Quarterly => 3,
        InstallmentFrequency.BiYearly => 6,
        _ => throw new ArgumentOutOfRangeException(nameof(frequency)),
    };

    public static List<PlannedInstallment> Build(
        decimal totalFee,
        int durationWeeks,
        DateOnly enrollmentDate,
        InstallmentFrequency frequency)
    {
        if (!IsFrequencyAllowed(frequency, durationWeeks))
            throw new ArgumentException($"Frequency {frequency} is not allowed for {durationWeeks} weeks.");

        var months = Math.Max(1, durationWeeks / 4);
        var interval = IntervalMonths(frequency);
        var count = (int)Math.Ceiling(months / (double)interval);
        if (count < 1) count = 1;

        var amountPer = Math.Round(totalFee / count, 2, MidpointRounding.AwayFromZero);
        var distributed = amountPer * (count - 1);
        var lastAmount = totalFee - distributed;

        var result = new List<PlannedInstallment>(count);
        for (var i = 0; i < count; i++)
        {
            var due = enrollmentDate.AddMonths(interval * (i + 1));
            var amount = i == count - 1 ? lastAmount : amountPer;
            result.Add(new PlannedInstallment(i + 1, amount, due));
        }
        return result;
    }
}

public record PlannedInstallment(int InstallmentNo, decimal AmountDue, DateOnly DueDate);
