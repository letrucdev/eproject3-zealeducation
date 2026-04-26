using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.ClassSessions.Queries.GetBatchSessions;

public class GetBatchSessionsQueryHandler(
    IRepository<Batch> batchRepository,
    IRepository<ClassSession> sessionRepository) : IRequestHandler<GetBatchSessionsQuery, List<ClassSessionDto>>
{
    public async Task<List<ClassSessionDto>> Handle(GetBatchSessionsQuery request, CancellationToken cancellationToken)
    {
        var batchExists = await batchRepository.ExistsAsync(request.BatchId, cancellationToken);
        if (!batchExists)
            throw new NotFoundException(nameof(Batch), request.BatchId);

        return await sessionRepository.Query()
            .Where(s => s.BatchId == request.BatchId)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .Select(s => new ClassSessionDto
            {
                SessionId = s.Id,
                BatchId = s.BatchId,
                SessionDate = s.SessionDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Topic = s.Topic,
                Location = s.Location,
                Status = s.Status,
                AttendanceMarkedCount = s.AttendanceRecords.Count
            })
            .ToListAsync(cancellationToken);
    }
}
