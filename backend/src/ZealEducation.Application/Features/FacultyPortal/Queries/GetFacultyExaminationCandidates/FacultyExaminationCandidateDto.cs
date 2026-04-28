namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyExaminationCandidates;

public class FacultyExaminationCandidateDto
{
    public Guid EnrollmentId { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateCode { get; set; } = default!;
    public string CandidateFullName { get; set; } = default!;
    public Guid? ResultId { get; set; }
    public decimal? Score { get; set; }
    public string? Grade { get; set; }
    public bool? IsPassed { get; set; }
    public bool IsOverridden { get; set; }
    public DateTime? GradedAt { get; set; }
}
