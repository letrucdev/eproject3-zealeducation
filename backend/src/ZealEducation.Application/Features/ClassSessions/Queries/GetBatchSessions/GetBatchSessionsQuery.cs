using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.ClassSessions.Queries.GetBatchSessions;

public record GetBatchSessionsQuery(
    Guid BatchId,
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<ClassSessionDto>>;
