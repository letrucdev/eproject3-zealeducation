using MediatR;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyCertificates;

public record GetMyCertificatesQuery() : IRequest<List<MyCertificateListItemDto>>;
