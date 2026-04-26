using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Candidates.Queries.GetCandidates;

public class CandidateListItemDto
{
    public Guid CandidateId { get; set; }
    public string CandidateCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public bool IsActive { get; set; }
    public CandidateStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }
    public Guid? CurrentEnrollmentId { get; set; }
    public Guid? CurrentCourseId { get; set; }
    public string? CurrentCourseName { get; set; }
    public Guid? CurrentBatchId { get; set; }
    public string? CurrentBatchCode { get; set; }
}
