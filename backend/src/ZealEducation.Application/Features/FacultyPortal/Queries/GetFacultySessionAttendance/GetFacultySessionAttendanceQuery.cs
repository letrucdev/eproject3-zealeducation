using MediatR;
using ZealEducation.Application.Features.ClassSessions.Queries.GetSessionAttendance;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultySessionAttendance;

public record GetFacultySessionAttendanceQuery(Guid SessionId) : IRequest<SessionAttendanceDto>;
