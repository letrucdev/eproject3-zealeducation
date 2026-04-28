using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.ClassSessions.Queries.GetSessionAttendance;

public class GetSessionAttendanceQueryHandler(
    IRepository<ClassSession> sessionRepository,
    IRepository<Batch> batchRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository,
    IRepository<AttendanceRecord> attendanceRepository) : IRequestHandler<GetSessionAttendanceQuery, SessionAttendanceDto>
{
    public async Task<SessionAttendanceDto> Handle(GetSessionAttendanceQuery request, CancellationToken cancellationToken)
    {
        var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ClassSession), request.SessionId);

        var batch = await batchRepository.GetByIdAsync(session.BatchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Batch), session.BatchId);

        var existingRecords = await attendanceRepository.Query()
            .Where(a => a.ClassSessionId == session.Id)
            .ToDictionaryAsync(a => a.EnrollmentId, cancellationToken);

        var rows = await (
            from enrollment in enrollmentRepository.Query()
            join candidate in candidateRepository.Query() on enrollment.CandidateId equals candidate.Id
            join user in userRepository.Query() on candidate.UserAccountId equals user.Id
            where enrollment.BatchId == session.BatchId
            orderby user.FullName, candidate.CandidateCode
            select new { enrollment, candidate, user })
            .ToListAsync(cancellationToken);

        var dto = new SessionAttendanceDto
        {
            SessionId = session.Id,
            BatchId = batch.Id,
            BatchCode = batch.BatchCode,
            SessionDate = session.SessionDate,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            Topic = session.Topic,
            Location = session.Location,
            Status = session.Status,
            Rows = [.. rows.Select(r =>
            {
                existingRecords.TryGetValue(r.enrollment.Id, out var record);
                return new AttendanceRowDto
                {
                    EnrollmentId = r.enrollment.Id,
                    CandidateId = r.candidate.Id,
                    CandidateCode = r.candidate.CandidateCode,
                    FullName = r.user.FullName,
                    Status = record?.Status,
                    PracticalHours = record?.PracticalHours,
                    Remarks = record?.Remarks
                };
            })]
        };

        return dto;
    }
}
