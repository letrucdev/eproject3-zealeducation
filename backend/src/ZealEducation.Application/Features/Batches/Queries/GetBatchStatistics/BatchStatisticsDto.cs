namespace ZealEducation.Application.Features.Batches.Queries.GetBatchStatistics;

public class BatchStatisticsDto
{
    public int Total { get; set; }
    public int NeedsInstructor { get; set; }
    public int Active { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
}
