using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.ClassSessions.Commands.UpdateClassSession;

public class UpdateClassSessionCommandHandler(
    IRepository<ClassSession> sessionRepository,
    IRepository<Batch> batchRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateClassSessionCommand, Unit>
{
    public async Task<Unit> Handle(UpdateClassSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ClassSession), request.SessionId);

        var batch = await batchRepository.GetByIdAsync(session.BatchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Batch), session.BatchId);

        if (request.SessionDate < batch.StartDate || request.SessionDate > batch.EndDate)
            throw new ConflictException(
                $"Session date must fall within the batch period ({batch.StartDate:yyyy-MM-dd} to {batch.EndDate:yyyy-MM-dd}).");

        var hasOverlap = await sessionRepository.Query()
            .AnyAsync(s => s.BatchId == batch.Id
                && s.Id != session.Id
                && s.SessionDate == request.SessionDate
                && s.StartTime < request.EndTime
                && request.StartTime < s.EndTime,
                cancellationToken);

        if (hasOverlap)
            throw new ConflictException("This session overlaps with another session on the same date.");

        session.SessionDate = request.SessionDate;
        session.StartTime = request.StartTime;
        session.EndTime = request.EndTime;
        session.Topic = string.IsNullOrWhiteSpace(request.Topic) ? null : request.Topic.Trim();
        session.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
        session.Status = request.Status;

        sessionRepository.Update(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
