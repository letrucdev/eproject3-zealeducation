using MediatR;
using ZealEducation.Application.Features.ClassSessions.Commands.MarkAttendance;

namespace ZealEducation.Application.Features.FacultyPortal.Commands.MarkFacultyAttendance;

public record MarkFacultyAttendanceCommand(
    Guid SessionId,
    List<AttendanceEntry> Entries) : IRequest<Unit>;
