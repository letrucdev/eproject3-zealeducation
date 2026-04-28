using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.Batches.Queries.GetBatches;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyBatches;

public record GetFacultyBatchesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    BatchStatus? Status = null,
    Guid? CourseId = null,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<BatchListItemDto>>;
