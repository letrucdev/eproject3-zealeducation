using MediatR;

namespace ZealEducation.Application.Features.Candidates.Queries.GetCandidateDetail;

public record GetCandidateDetailQuery(Guid CandidateId) : IRequest<CandidateDetailDto>;
