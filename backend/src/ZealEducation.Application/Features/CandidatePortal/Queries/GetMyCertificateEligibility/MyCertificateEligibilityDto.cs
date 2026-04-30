using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyCertificateEligibility;

public class MyCertificateEligibilityDto
{
    public Guid BatchId { get; set; }
    public bool IsEligible { get; set; }
    public bool FeesPaid { get; set; }
    public decimal AttendancePercent { get; set; }
    public bool AttendanceOk { get; set; }
    public bool ExamsPassed { get; set; }
    public string? Reason { get; set; }
    public CertificateApplicationStatus? ExistingApplicationStatus { get; set; }
}
