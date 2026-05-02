using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.CertificateApplications.Queries.GetCertificateApplications;

public record GetCertificateApplicationsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    CertificateApplicationStatus? Status = null,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<CertificateApplicationListItemDto>>;
