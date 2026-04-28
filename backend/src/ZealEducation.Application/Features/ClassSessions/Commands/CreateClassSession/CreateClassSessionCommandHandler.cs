using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.ClassSessions.Commands.CreateClassSession;

public class CreateClassSessionCommandHandler(
    IRepository<Batch> batchRepository,
    IRepository<ClassSession> sessionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateClassSessionCommand, Guid>
{
    public async Task<Guid> Handle(CreateClassSessionCommand request, CancellationToken cancellationToken)
    {
        var batch = await batchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Batch), request.BatchId);

        if (batch.Status is BatchStatus.Completed or BatchStatus.Cancelled)
            throw new ConflictException("Cannot add sessions to a completed or cancelled batch.");

        if (request.SessionDate < batch.StartDate || request.SessionDate > batch.EndDate)
            throw new ConflictException(
                $"Session date must fall within the batch period ({batch.StartDate:yyyy-MM-dd} to {batch.EndDate:yyyy-MM-dd}).");

        var hasOverlap = await sessionRepository.Query()
            .AnyAsync(s => s.BatchId == batch.Id
                && s.SessionDate == request.SessionDate
                && s.StartTime < request.EndTime
                && request.StartTime < s.EndTime,
                cancellationToken);

        if (hasOverlap)
            throw new ConflictException("This session overlaps with another session on the same date.");

        var session = new ClassSession
        {
            Id = Guid.NewGuid(),
            BatchId = batch.Id,
            SessionDate = request.SessionDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Topic = string.IsNullOrWhiteSpace(request.Topic) ? null : request.Topic.Trim(),
            Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
            Status = ClassSessionStatus.Scheduled
        };

        await sessionRepository.AddAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return session.Id;
    }
}
