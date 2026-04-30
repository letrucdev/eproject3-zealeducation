using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyCertificates;

public class MyCertificateListItemDto
{
    public Guid ApplicationId { get; set; }
    public Guid? BatchId { get; set; }
    public string? BatchCode { get; set; }
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = default!;
    public CertificateApplicationStatus Status { get; set; }
    public string? CertificateNumber { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
