using MediatR;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CandidatePortal.Common;
using ZealEducation.Application.Features.Candidates.Queries.GetCandidateDetail;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyProfile;

public class GetMyProfileQueryHandler(
    ICurrentUser currentUser,
    IRepository<Candidate> candidateRepository,
    ISender sender) : IRequestHandler<GetMyProfileQuery, CandidateDetailDto>
{
    public async Task<CandidateDetailDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateResolver.ResolveAsync(currentUser, candidateRepository, cancellationToken);
        return await sender.Send(new GetCandidateDetailQuery(candidate.Id), cancellationToken);
    }
}
