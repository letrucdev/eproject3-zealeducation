using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.Batches.Queries.GetBatchEnrollments;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyBatchEnrollments;

public record GetFacultyBatchEnrollmentsQuery(
    Guid BatchId,
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<BatchEnrollmentItemDto>>;
