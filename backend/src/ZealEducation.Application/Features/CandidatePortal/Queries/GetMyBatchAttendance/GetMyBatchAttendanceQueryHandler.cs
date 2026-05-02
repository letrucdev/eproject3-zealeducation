using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.CandidatePortal.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchAttendance;

public class GetMyBatchAttendanceQueryHandler(
    ICurrentUser currentUser,
    IRepository<Candidate> candidateRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<ClassSession> sessionRepository,
    IRepository<AttendanceRecord> attendanceRepository) : IRequestHandler<GetMyBatchAttendanceQuery, MyBatchAttendanceDto>
{
    public async Task<MyBatchAttendanceDto> Handle(GetMyBatchAttendanceQuery request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateResolver.ResolveAsync(currentUser, candidateRepository, cancellationToken);

        var enrollment = await enrollmentRepository.Query()
            .Where(e => e.CandidateId == candidate.Id && e.BatchId == request.BatchId)
            .Select(e => new { e.Id })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("You are not enrolled in this batch.");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        // Filter sessions theo BatchId va date range (neu co). Aggregate va rows cung dung scope nay
        // de badge tong/cot table luon nhat quan voi nhau.
        var sessionsScope = sessionRepository.Query().Where(s => s.BatchId == request.BatchId);

        if (request.FromDate.HasValue)
        {
            var from = request.FromDate.Value;
            sessionsScope = sessionsScope.Where(s => s.SessionDate >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = request.ToDate.Value;
            sessionsScope = sessionsScope.Where(s => s.SessionDate <= to);
        }

        // Aggregates tinh tren toan bo sessions trong scope (khong phan theo trang).
        var totals = await (
            from s in sessionsScope
            join a in attendanceRepository.Query().Where(x => x.EnrollmentId == enrollment.Id)
                on s.Id equals a.ClassSessionId into joined
            from a in joined.DefaultIfEmpty()
            select new
            {
                AttendanceStatus = a != null ? (AttendanceStatus?)a.Status : null,
                PracticalHours = a != null ? a.PracticalHours : null
            }).ToListAsync(cancellationToken);

        // Left join sessions <-> attendance, sap xep va phan trang phia DB.
        var rowsQuery =
            from s in sessionsScope
            join a in attendanceRepository.Query().Where(x => x.EnrollmentId == enrollment.Id)
                on s.Id equals a.ClassSessionId into joined
            from a in joined.DefaultIfEmpty()
            select new MyAttendanceRowDto
            {
                SessionId = s.Id,
                SessionDate = s.SessionDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Topic = s.Topic,
                Location = s.Location,
                SessionStatus = s.Status,
                AttendanceStatus = a != null ? (AttendanceStatus?)a.Status : null,
                PracticalHours = a != null ? a.PracticalHours : null,
                Remarks = a != null ? a.Remarks : null
            };

        var sortKey = (request.SortBy ?? string.Empty).Trim().ToLower();
        var direction = (request.SortDirection ?? "asc").Trim().ToLower();

        var ordered = (sortKey, direction) switch
        {
            ("topic", "desc") => rowsQuery.OrderByDescending(r => r.Topic),
            ("topic", _) => rowsQuery.OrderBy(r => r.Topic),
            ("status", "desc") => rowsQuery.OrderByDescending(r => r.AttendanceStatus),
            ("status", _) => rowsQuery.OrderBy(r => r.AttendanceStatus),
            ("practicalhours", "desc") => rowsQuery.OrderByDescending(r => r.PracticalHours),
            ("practicalhours", _) => rowsQuery.OrderBy(r => r.PracticalHours),
            ("sessiondate", "desc") => rowsQuery.OrderByDescending(r => r.SessionDate),
            _ => rowsQuery.OrderBy(r => r.SessionDate),
        };

        var paginated = await PaginatedList<MyAttendanceRowDto>.CreateAsync(
            ordered.ThenBy(r => r.StartTime),
            page,
            pageSize);

        return new MyBatchAttendanceDto
        {
            BatchId = request.BatchId,
            EnrollmentId = enrollment.Id,
            TotalPracticalHours = totals.Sum(t => t.PracticalHours ?? 0m),
            TotalSessions = totals.Count,
            PresentCount = totals.Count(t => t.AttendanceStatus == AttendanceStatus.Present),
            AbsentCount = totals.Count(t => t.AttendanceStatus == AttendanceStatus.Absent),
            LateCount = totals.Count(t => t.AttendanceStatus == AttendanceStatus.Late),
            Rows = paginated
        };
    }
}
