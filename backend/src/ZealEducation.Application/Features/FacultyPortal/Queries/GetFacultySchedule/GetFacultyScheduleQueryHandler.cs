using MediatR;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;
using FacultyEntity = ZealEducation.Domain.Entities.Faculty;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultySchedule;

public class GetFacultyScheduleQueryHandler(
    IRepository<ClassSession> sessionRepository,
    IRepository<FacultyEntity> facultyRepository,
    ICurrentUser currentUser) : IRequestHandler<GetFacultyScheduleQuery, PaginatedList<FacultyScheduleItemDto>>
{
    public async Task<PaginatedList<FacultyScheduleItemDto>> Handle(GetFacultyScheduleQuery request, CancellationToken cancellationToken)
    {
        var facultyId = await facultyRepository.ResolveFacultyIdAsync(currentUser, cancellationToken);

        var from = request.From;
        var to = request.To;
        if (to < from)
            (from, to) = (to, from);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var query = sessionRepository.Query()
            .Where(s =>
                s.Batch.FacultyId == facultyId &&
                s.Batch.Status == BatchStatus.Active &&
                s.SessionDate >= from &&
                s.SessionDate <= to);

        if (request.BatchId.HasValue)
        {
            var batchId = request.BatchId.Value;
            query = query.Where(s => s.BatchId == batchId);
        }

        if (request.CourseId.HasValue)
        {
            var courseId = request.CourseId.Value;
            query = query.Where(s => s.Batch.CourseId == courseId);
        }

        var sortKey = (request.SortBy ?? string.Empty).Trim().ToLower();
        var direction = (request.SortDirection ?? "asc").Trim().ToLower();

        var ordered = (sortKey, direction) switch
        {
            ("sessiondate", "desc") => query.OrderByDescending(s => s.SessionDate).ThenBy(s => s.StartTime),
            ("starttime", "desc") => query.OrderByDescending(s => s.StartTime).ThenBy(s => s.SessionDate),
            ("starttime", _) => query.OrderBy(s => s.StartTime).ThenBy(s => s.SessionDate),
            ("batchcode", "desc") => query.OrderByDescending(s => s.Batch.BatchCode).ThenBy(s => s.SessionDate).ThenBy(s => s.StartTime),
            ("batchcode", _) => query.OrderBy(s => s.Batch.BatchCode).ThenBy(s => s.SessionDate).ThenBy(s => s.StartTime),
            ("coursename", "desc") => query.OrderByDescending(s => s.Batch.Course.CourseName).ThenBy(s => s.SessionDate).ThenBy(s => s.StartTime),
            ("coursename", _) => query.OrderBy(s => s.Batch.Course.CourseName).ThenBy(s => s.SessionDate).ThenBy(s => s.StartTime),
            ("status", "desc") => query.OrderByDescending(s => s.Status).ThenBy(s => s.SessionDate).ThenBy(s => s.StartTime),
            ("status", _) => query.OrderBy(s => s.Status).ThenBy(s => s.SessionDate).ThenBy(s => s.StartTime),
            _ => query.OrderBy(s => s.SessionDate).ThenBy(s => s.StartTime),
        };

        var projected = ordered.Select(s => new FacultyScheduleItemDto
        {
            SessionId = s.Id,
            BatchId = s.BatchId,
            BatchCode = s.Batch.BatchCode,
            CourseName = s.Batch.Course.CourseName,
            SessionDate = s.SessionDate,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Topic = s.Topic,
            Location = s.Location,
            Status = s.Status,
        });

        return await PaginatedList<FacultyScheduleItemDto>.CreateAsync(projected, page, pageSize);
    }
}
