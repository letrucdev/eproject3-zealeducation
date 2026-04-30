using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.CertificateApplications.Queries.GetCertificateApplications;

public class CertificateApplicationListItemDto
{
    public Guid ApplicationId { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateCode { get; set; } = default!;
    public string CandidateName { get; set; } = default!;
    public Guid? BatchId { get; set; }
    public string? BatchCode { get; set; }
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = default!;
    public CertificateApplicationStatus Status { get; set; }
    public string? CertificateNumber { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedByStaffId { get; set; }
    public string? ApprovedByName { get; set; }

    public bool FeesPaid { get; set; }
    public decimal AttendancePercent { get; set; }
    public bool AttendanceOk { get; set; }
    public bool ExamsPassed { get; set; }
    public bool IsCurrentlyEligible { get; set; }
}
