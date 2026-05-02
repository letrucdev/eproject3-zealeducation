using MediatR;

namespace ZealEducation.Application.Features.Candidates.Queries.GetCandidateRegistrationsTrend;

public record GetCandidateRegistrationsTrendQuery(int Days) : IRequest<List<CandidateRegistrationTrendPointDto>>;
