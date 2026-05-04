using ZealEducation.Application.Features.Payments.Common;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Candidates.Notifications;

public class CourseAddedEmailModel
{
    public string RecipientEmail { get; init; } = default!;
    public string RecipientName { get; init; } = default!;

    public string CandidateCode { get; init; } = default!;

    public string CourseName { get; init; } = default!;
    public int DurationWeeks { get; init; }
    public decimal BaseFee { get; init; }

    public DateOnly EnrollmentDate { get; init; }

    public decimal LumpSumAmount { get; init; }
    public IReadOnlyList<CourseAddedInstallmentOption> InstallmentOptions { get; init; } = [];
}

public class CourseAddedInstallmentOption
{
    public InstallmentFrequency Frequency { get; init; }
    public IReadOnlyList<PlannedInstallment> Items { get; init; } = [];
}
