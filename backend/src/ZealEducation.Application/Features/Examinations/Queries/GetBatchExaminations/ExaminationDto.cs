namespace ZealEducation.Application.Features.Examinations.Queries.GetBatchExaminations;

public class ExaminationDto
{
    public Guid ExaminationId { get; set; }
    public Guid BatchId { get; set; }
    public string ExamName { get; set; } = default!;
    public DateOnly ExamDate { get; set; }
    public string? Location { get; set; }
    public int MaxScore { get; set; }
    public int PassScore { get; set; }
    public Guid ScheduledById { get; set; }
    public string ScheduledByName { get; set; } = default!;
    public bool HasResults { get; set; }
    public DateTime CreatedAt { get; set; }
}
