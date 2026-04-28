using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultySchedule;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;
using FacultyEntity = ZealEducation.Domain.Entities.Faculty;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyUpcomingSessions;

public class GetFacultyUpcomingSessionsQueryHandler(
    IRepository<ClassSession> sessionRepository,
    IRepository<FacultyEntity> facultyRepository,
    ICurrentUser currentUser) : IRequestHandler<GetFacultyUpcomingSessionsQuery, List<FacultyScheduleItemDto>>
{
    private static readonly TimeSpan UpcomingWindow = TimeSpan.FromHours(3);

    public async Task<List<FacultyScheduleItemDto>> Handle(GetFacultyUpcomingSessionsQuery request, CancellationToken cancellationToken)
    {
        var facultyId = await facultyRepository.ResolveFacultyIdAsync(currentUser, cancellationToken);

        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(nowUtc);
        var nowTime = TimeOnly.FromDateTime(nowUtc);

        // Window có thể trượt qua nửa đêm (vd: 23:00 + 3h = 02:00 hôm sau).
        // Tách thành 2 điều kiện theo ngày để LINQ dịch sang SQL gọn.
        var endTime = nowTime.AddHours(UpcomingWindow.TotalHours);
        var crossesMidnight = endTime < nowTime;
        var tomorrow = today.AddDays(1);

        var query = sessionRepository.Query()
            .Where(s =>
                s.Batch.FacultyId == facultyId &&
                s.Batch.Status == BatchStatus.Active &&
                s.Status == ClassSessionStatus.Scheduled);

        query = crossesMidnight
            ? query.Where(s =>
                (s.SessionDate == today && s.StartTime >= nowTime) ||
                (s.SessionDate == tomorrow && s.StartTime <= endTime))
            : query.Where(s =>
                s.SessionDate == today &&
                s.StartTime >= nowTime &&
                s.StartTime <= endTime);

        return await query
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .Select(s => new FacultyScheduleItemDto
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
            })
            .ToListAsync(cancellationToken);
    }
}
