using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Candidates.Queries.GetCandidates;

public record GetCandidatesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    CandidateStatus? Status = null,
    Guid? CourseId = null,
    Guid? BatchId = null) : IRequest<PaginatedList<CandidateListItemDto>>;
