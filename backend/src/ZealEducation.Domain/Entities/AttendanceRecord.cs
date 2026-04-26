using ZealEducation.Domain.Common;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Domain.Entities;

public class AttendanceRecord : BaseAuditableEntity
{
    public Guid ClassSessionId { get; set; }
    public Guid EnrollmentId { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }

    public ClassSession ClassSession { get; set; } = default!;
    public Enrollment Enrollment { get; set; } = default!;
}
