namespace ZealEducation.Application.Features.Examinations.Queries.GetBatchGradeDistribution;

public class BatchGradeDistributionDto
{
    public int A { get; set; }
    public int B { get; set; }
    public int C { get; set; }
    public int D { get; set; }
    public int F { get; set; }
    public int Ungraded { get; set; }
    public int Total { get; set; }
}
