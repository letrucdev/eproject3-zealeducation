namespace ZealEducation.Application.Features.Batches.Queries.GetBatchCreationTrend;

public class BatchCreationTrendPointDto
{
    public DateOnly Date { get; set; }
    public int Total { get; set; }
    public int Active { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
}
