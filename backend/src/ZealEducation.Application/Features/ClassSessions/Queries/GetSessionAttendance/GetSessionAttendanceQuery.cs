using MediatR;

namespace ZealEducation.Application.Features.ClassSessions.Queries.GetSessionAttendance;

public record GetSessionAttendanceQuery(Guid SessionId) : IRequest<SessionAttendanceDto>;
