using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.Batches.Queries.GetBatchEnrollments;

public record GetBatchEnrollmentsQuery(
    Guid BatchId,
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<BatchEnrollmentItemDto>>;
