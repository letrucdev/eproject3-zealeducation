namespace ZealEducation.Application.Features.ExamResults.Queries.GetExaminationResults;

public class ExamResultDto
{
    public Guid ResultId { get; set; }
    public Guid ExamId { get; set; }
    public Guid EnrollmentId { get; set; }
    public string CandidateCode { get; set; } = default!;
    public string CandidateFullName { get; set; } = default!;
    public decimal Score { get; set; }
    public string? Grade { get; set; }
    public bool IsPassed { get; set; }
    public bool IsFinalized { get; set; }
    public Guid GradedById { get; set; }
    public string GradedByName { get; set; } = default!;
    public bool IsOverridden { get; set; }
    public Guid? OverrideById { get; set; }
    public string? OverrideByName { get; set; }
    public string? OverrideReason { get; set; }
    public DateTime GradedAt { get; set; }
}
