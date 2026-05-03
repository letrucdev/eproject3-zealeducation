namespace ZealEducation.Application.Features.Candidates.Queries.GetCandidateRegistrationsTrend;

public class CandidateRegistrationTrendPointDto
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
    public int Graduated { get; set; }
    public int Dropped { get; set; }
}
