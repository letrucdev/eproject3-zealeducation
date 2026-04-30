using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Common.Helpers;

public record CertificateEligibilityResult(
    bool IsEligible,
    bool FeesPaid,
    decimal AttendancePercent,
    bool AttendanceOk,
    bool ExamsPassed,
    string? Reason);

public static class CertificateEligibilityChecker
{
    public const decimal MinAttendancePercent = 80m;

    public static CertificateEligibilityResult Evaluate(
        FeeStructure? fee,
        int presentCount,
        int totalSessions,
        IReadOnlyList<ExamResult> examResults)
    {
        var feesPaid = fee is { PaymentStatus: PaymentStatus.Paid, OutstandingBalance: 0m };

        var attendancePercent = totalSessions > 0
            ? Math.Round((decimal)presentCount / totalSessions * 100m, 2)
            : 0m;
        var attendanceOk = attendancePercent >= MinAttendancePercent;

        var examsPassed = examResults.Count > 0
            && examResults.All(r => r.IsFinalized && r.IsPassed);

        var isEligible = feesPaid && attendanceOk && examsPassed;

        string? reason = null;
        if (!isEligible)
        {
            var parts = new List<string>();
            if (!feesPaid) parts.Add("course fees are not fully paid");
            if (!attendanceOk) parts.Add($"attendance is below {MinAttendancePercent:0}% (current: {attendancePercent:0.##}%)");
            if (!examsPassed) parts.Add("not all exams have been finalized and passed");
            reason = "Not eligible: " + string.Join("; ", parts) + ".";
        }

        return new CertificateEligibilityResult(
            isEligible,
            feesPaid,
            attendancePercent,
            attendanceOk,
            examsPassed,
            reason);
    }
}
