using MediatR;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchAttendance;

public record GetMyBatchAttendanceQuery(
    Guid BatchId,
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    string? SortDirection = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null) : IRequest<MyBatchAttendanceDto>;
