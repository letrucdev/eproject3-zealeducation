namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyExaminations;

public class FacultyExaminationSummaryDto
{
    public Guid ExaminationId { get; set; }
    public Guid BatchId { get; set; }
    public string BatchCode { get; set; } = default!;
    public string CourseName { get; set; } = default!;
    public string ExamName { get; set; } = default!;
    public DateOnly ExamDate { get; set; }
    public string? Location { get; set; }
    public int MaxScore { get; set; }
    public int PassScore { get; set; }
    public int ResultCount { get; set; }
    public decimal? AverageScore { get; set; }
    public decimal? MinScore { get; set; }
    public decimal? MaxStudentScore { get; set; }
    public DateTime CreatedAt { get; set; }
}
