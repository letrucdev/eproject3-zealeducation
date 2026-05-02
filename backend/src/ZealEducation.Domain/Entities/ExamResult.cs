using ZealEducation.Domain.Common;

namespace ZealEducation.Domain.Entities;

public class ExamResult : BaseAuditableEntity
{
    public Guid ExamId { get; set; }
    public Guid EnrollmentId { get; set; }
    public decimal Score { get; set; }
    public string? Grade { get; set; }
    public bool IsPassed { get; set; }
    public bool IsFinalized { get; set; }
    public Guid GradedById { get; set; }
    public bool IsOverridden { get; set; }
    public Guid? OverrideById { get; set; }
    public string? OverrideReason { get; set; }
    public DateTime GradedAt { get; set; }

    public Examination Examination { get; set; } = default!;
    public Enrollment Enrollment { get; set; } = default!;
    public Staff GradedBy { get; set; } = default!;
    public Staff? OverrideBy { get; set; }
}
