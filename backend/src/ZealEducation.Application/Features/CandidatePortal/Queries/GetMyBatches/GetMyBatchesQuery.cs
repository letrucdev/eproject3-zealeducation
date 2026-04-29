using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatches;

public record GetMyBatchesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<MyBatchListItemDto>>;
