namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchExamResults;

public class MyBatchExamResultsDto
{
    public Guid BatchId { get; set; }
    public Guid EnrollmentId { get; set; }
    public List<MyExamResultRowDto> Rows { get; set; } = [];
}

public class MyExamResultRowDto
{
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = default!;
    public DateOnly ExamDate { get; set; }
    public int MaxScore { get; set; }
    public int PassScore { get; set; }
    public string? Location { get; set; }
    public Guid? ResultId { get; set; }
    public decimal? Score { get; set; }
    public string? Grade { get; set; }
    public bool? IsPassed { get; set; }
    public bool IsOverridden { get; set; }
    public DateTime? GradedAt { get; set; }
}
