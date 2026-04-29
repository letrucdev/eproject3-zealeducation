using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.CandidatePortal.Common;
using ZealEducation.Application.Features.ClassSessions.Queries.GetBatchSessions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchSessions;

public class GetMyBatchSessionsQueryHandler(
    ICurrentUser currentUser,
    IRepository<Candidate> candidateRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<ClassSession> sessionRepository) : IRequestHandler<GetMyBatchSessionsQuery, PaginatedList<ClassSessionDto>>
{
    public async Task<PaginatedList<ClassSessionDto>> Handle(GetMyBatchSessionsQuery request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateResolver.ResolveAsync(currentUser, candidateRepository, cancellationToken);

        var enrolled = await enrollmentRepository.Query()
            .AnyAsync(e => e.CandidateId == candidate.Id && e.BatchId == request.BatchId, cancellationToken);
        if (!enrolled)
            throw new NotFoundException("You are not enrolled in this batch.");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        var query = sessionRepository.Query().Where(s => s.BatchId == request.BatchId);

        if (request.FromDate.HasValue)
        {
            var from = request.FromDate.Value;
            query = query.Where(s => s.SessionDate >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = request.ToDate.Value;
            query = query.Where(s => s.SessionDate <= to);
        }

        var sortKey = (request.SortBy ?? string.Empty).Trim().ToLower();
        var direction = (request.SortDirection ?? "asc").Trim().ToLower();

        var ordered = (sortKey, direction) switch
        {
            ("topic", "desc") => query.OrderByDescending(s => s.Topic),
            ("topic", _) => query.OrderBy(s => s.Topic),
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
