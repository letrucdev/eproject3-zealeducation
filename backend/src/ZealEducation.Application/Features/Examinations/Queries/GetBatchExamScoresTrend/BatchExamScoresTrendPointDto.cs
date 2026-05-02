namespace ZealEducation.Application.Features.Examinations.Queries.GetBatchExamScoresTrend;

public class BatchExamScoresTrendPointDto
{
    public DateOnly Date { get; set; }
    public decimal AverageScore { get; set; }
    public decimal HighestScore { get; set; }
    public decimal LowestScore { get; set; }
    public int Count { get; set; }
}
