using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.ClassSessions.Commands.MarkAttendance;

public class MarkAttendanceCommandHandler(
    IRepository<ClassSession> sessionRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<AttendanceRecord> attendanceRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkAttendanceCommand, Unit>
{
    public async Task<Unit> Handle(MarkAttendanceCommand request, CancellationToken cancellationToken)
    {
        var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ClassSession), request.SessionId);

        var entries = request.Entries
            .GroupBy(e => e.EnrollmentId)
            .Select(g => g.Last())
            .ToList();

        var enrollmentIds = entries.Select(e => e.EnrollmentId).ToList();

        var validEnrollmentIds = await enrollmentRepository.Query()
            .Where(e => enrollmentIds.Contains(e.Id) && e.BatchId == session.BatchId)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        if (validEnrollmentIds.Count != enrollmentIds.Count)
            throw new ConflictException("One or more enrollments do not belong to this session's batch.");

        var maxPracticalHours = Math.Round((decimal)(session.EndTime - session.StartTime).TotalHours, 2);
        if (entries.Any(e => e.PracticalHours.HasValue && e.PracticalHours.Value > maxPracticalHours))
            throw new ConflictException(
                $"Practical hours cannot exceed the session duration ({maxPracticalHours:0.##} hours).");

        var existing = await attendanceRepository.Query()
            .Where(a => a.ClassSessionId == session.Id)
            .ToListAsync(cancellationToken);

        var existingByEnrollment = existing.ToDictionary(a => a.EnrollmentId);

        foreach (var entry in entries)
        {
            var remarks = string.IsNullOrWhiteSpace(entry.Remarks) ? null : entry.Remarks.Trim();

            if (existingByEnrollment.TryGetValue(entry.EnrollmentId, out var record))
            {
                record.Status = entry.Status;
                record.PracticalHours = entry.PracticalHours;
                record.Remarks = remarks;
                attendanceRepository.Update(record);
            }
            else
            {
                var newRecord = new AttendanceRecord
                {
                    Id = Guid.NewGuid(),
                    ClassSessionId = session.Id,
                    EnrollmentId = entry.EnrollmentId,
                    Status = entry.Status,
                    PracticalHours = entry.PracticalHours,
                    Remarks = remarks
                };
                await attendanceRepository.AddAsync(newRecord, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
