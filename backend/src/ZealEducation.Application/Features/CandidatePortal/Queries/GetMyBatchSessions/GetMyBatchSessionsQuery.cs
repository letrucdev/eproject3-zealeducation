using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.ClassSessions.Queries.GetBatchSessions;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchSessions;

public record GetMyBatchSessionsQuery(
    Guid BatchId,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDirection = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null) : IRequest<PaginatedList<ClassSessionDto>>;
