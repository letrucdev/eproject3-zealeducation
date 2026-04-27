using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.ClassSessions.Commands.CreateBulkSessions;

public class CreateBulkSessionsCommandHandler(
    IRepository<Batch> batchRepository,
    IRepository<ClassSession> sessionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBulkSessionsCommand, CreateBulkSessionsResponse>
{
    public async Task<CreateBulkSessionsResponse> Handle(
        CreateBulkSessionsCommand request,
        CancellationToken cancellationToken)
    {
        var batch = await batchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Batch), request.BatchId);

        if (batch.Status is BatchStatus.Completed or BatchStatus.Cancelled)
            throw new ConflictException("Cannot add sessions to a completed or cancelled batch.");

        var selectedDays = request.DaysOfWeek.ToHashSet();

        var existing = await sessionRepository.Query()
            .Where(s => s.BatchId == batch.Id
                && s.SessionDate >= batch.StartDate
                && s.SessionDate <= batch.EndDate)
            .Select(s => new { s.SessionDate, s.StartTime, s.EndTime })
            .ToListAsync(cancellationToken);

        var topic = string.IsNullOrWhiteSpace(request.Topic) ? null : request.Topic.Trim();
        var location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();

        var skippedDates = new List<DateOnly>();
        var created = new List<ClassSession>();

        for (var date = batch.StartDate; date <= batch.EndDate; date = date.AddDays(1))
        {
            if (!selectedDays.Contains(date.DayOfWeek))
                continue;

            var capturedDate = date;
            var hasOverlap = existing.Any(e =>
                e.SessionDate == capturedDate
                && e.StartTime < request.EndTime
                && request.StartTime < e.EndTime);

            if (hasOverlap)
            {
                skippedDates.Add(capturedDate);
                continue;
            }

            var session = new ClassSession
            {
                Id = Guid.NewGuid(),
                BatchId = batch.Id,
                SessionDate = capturedDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Topic = topic,
                Location = location,
                Status = ClassSessionStatus.Scheduled
            };

            await sessionRepository.AddAsync(session, cancellationToken);
            created.Add(session);
        }

        if (created.Count > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateBulkSessionsResponse(created.Count, skippedDates);
    }
}
