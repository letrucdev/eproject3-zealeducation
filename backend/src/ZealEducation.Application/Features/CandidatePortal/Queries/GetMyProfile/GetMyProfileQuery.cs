using MediatR;
using ZealEducation.Application.Features.Candidates.Queries.GetCandidateDetail;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyProfile;

public record GetMyProfileQuery() : IRequest<CandidateDetailDto>;
