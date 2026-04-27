using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Batches.Queries.GetBatchEnrollments;

public class BatchEnrollmentItemDto
{
    public Guid EnrollmentId { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public DateOnly EnrollmentDate { get; set; }
    public EnrollmentStatus Status { get; set; }
}
