using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Batches.Queries.GetBatches;

public record GetBatchesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    Guid? CourseId = null,
    Guid? FacultyId = null,
    BatchStatus? Status = null) : IRequest<PaginatedList<BatchListItemDto>>;
