using ZealEducation.Domain.Common;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Domain.Entities;

public class CertificateApplication : BaseAuditableEntity
{
    public Guid EnrollmentId { get; set; }
    public CertificateApplicationStatus Status { get; set; } = CertificateApplicationStatus.Pending;

    public string? CertificateNumber { get; set; }
    public string? CertificateFilePath { get; set; }

    public Guid? ApprovedByStaffId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public Enrollment Enrollment { get; set; } = default!;
    public Staff? ApprovedByStaff { get; set; }
}
