using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.ClassSessions.Queries.GetBatchSessions;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyBatchSessions;

public record GetFacultyBatchSessionsQuery(
    Guid BatchId,
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<ClassSessionDto>>;
