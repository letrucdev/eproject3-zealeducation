using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.ClassSessions.Queries.GetBatchSessions;

public class GetBatchSessionsQueryHandler(
    IRepository<Batch> batchRepository,
    IRepository<ClassSession> sessionRepository) : IRequestHandler<GetBatchSessionsQuery, PaginatedList<ClassSessionDto>>
{
    public async Task<PaginatedList<ClassSessionDto>> Handle(GetBatchSessionsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var batchExists = await batchRepository.ExistsAsync(request.BatchId, cancellationToken);
        if (!batchExists)
            throw new NotFoundException(nameof(Batch), request.BatchId);

        var query = sessionRepository.Query().Where(s => s.BatchId == request.BatchId);

        var sortKey = (request.SortBy ?? string.Empty).Trim().ToLower();
        var direction = (request.SortDirection ?? "asc").Trim().ToLower();

        var ordered = (sortKey, direction) switch
        {
            ("topic", "desc") => query.OrderByDescending(s => s.Topic),
            ("topic", _) => query.OrderBy(s => s.Topic),
            ("location", "desc") => query.OrderByDescending(s => s.Location),
            ("location", _) => query.OrderBy(s => s.Location),
            ("status", "desc") => query.OrderByDescending(s => s.Status),
            ("status", _) => query.OrderBy(s => s.Status),
            ("sessiondate", "desc") => query.OrderByDescending(s => s.SessionDate),
            _ => query.OrderBy(s => s.SessionDate),
        };

        var projected = ordered
            .ThenBy(s => s.StartTime)
            .Select(s => new ClassSessionDto
            {
                SessionId = s.Id,
                BatchId = s.BatchId,
                SessionDate = s.SessionDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Topic = s.Topic,
                Location = s.Location,
                Status = s.Status,
                AttendanceMarkedCount = s.AttendanceRecords.Count
            });

        return await PaginatedList<ClassSessionDto>.CreateAsync(projected, page, pageSize);
    }
}
