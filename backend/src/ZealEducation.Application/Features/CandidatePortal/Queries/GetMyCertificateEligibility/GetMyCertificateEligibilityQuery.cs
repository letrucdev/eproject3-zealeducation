using MediatR;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyCertificateEligibility;

public record GetMyCertificateEligibilityQuery(Guid BatchId) : IRequest<MyCertificateEligibilityDto>;
