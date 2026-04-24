using ZealEducation.Domain.Common;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Domain.Entities;

public class Enrollment : BaseAuditableEntity
{
    public Guid CandidateId { get; set; }
    public Guid? BatchId { get; set; }
    public Guid? InchargeId { get; set; }
    public DateOnly EnrollmentDate { get; set; }
    public Guid CourseId { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.PendingAssignment;
    public string? Notes { get; set; }

    public Candidate Candidate { get; set; } = default!;
    public Course Course { get; set; } = default!;
    public Batch? Batch { get; set; }
    public Staff? Incharge { get; set; }
}
