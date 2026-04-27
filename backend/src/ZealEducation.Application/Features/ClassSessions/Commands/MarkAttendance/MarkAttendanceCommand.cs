using MediatR;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.ClassSessions.Commands.MarkAttendance;

public record MarkAttendanceCommand(
    Guid SessionId,
    List<AttendanceEntry> Entries) : IRequest<Unit>;

public record AttendanceEntry(
    Guid EnrollmentId,
    AttendanceStatus Status,
    string? Remarks);
