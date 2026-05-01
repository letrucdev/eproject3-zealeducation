using ZealEducation.Application.Features.Payments.Common;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.CourseEnquiries.Notifications;

public class EnquiryConvertedEmailModel
{
    public string RecipientEmail { get; init; } = default!;
    public string RecipientName { get; init; } = default!;

    public string Username { get; init; } = default!;
    public string TemporaryPassword { get; init; } = default!;

    public string CandidateCode { get; init; } = default!;
    public DateTime ConvertedAt { get; init; }

    public string CourseName { get; init; } = default!;
    public int DurationWeeks { get; init; }
    public decimal BaseFee { get; init; }

    public decimal LumpSumAmount { get; init; }
    public IReadOnlyList<EnquiryConvertedInstallmentOption> InstallmentOptions { get; init; } = [];
}

public class EnquiryConvertedInstallmentOption
{
    public InstallmentFrequency Frequency { get; init; }
    public IReadOnlyList<PlannedInstallment> Items { get; init; } = [];
}
