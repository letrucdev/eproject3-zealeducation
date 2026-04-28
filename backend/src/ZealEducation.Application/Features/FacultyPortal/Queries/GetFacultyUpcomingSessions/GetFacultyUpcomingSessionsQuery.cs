using MediatR;
using ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultySchedule;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyUpcomingSessions;

public record GetFacultyUpcomingSessionsQuery() : IRequest<List<FacultyScheduleItemDto>>;
