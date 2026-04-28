using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Batches.Commands.UpdateBatch;

public class UpdateBatchCommandHandler(
    IRepository<Batch> batchRepository,
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateBatchCommand, Unit>
{
    public async Task<Unit> Handle(UpdateBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await batchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Batch), request.BatchId);

        if (request.StartDate != batch.StartDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            if (request.StartDate < today)
                throw new ConflictException("Start date must be today or later.");
        }

        var batchCode = request.BatchCode.Trim();

        if (!string.Equals(batch.BatchCode, batchCode, StringComparison.Ordinal))
        {
            var duplicates = await batchRepository.FindAsync(
                b => b.BatchCode == batchCode && b.Id != batch.Id,
                cancellationToken);
            if (duplicates.Count > 0)
                throw new ConflictException("Batch code is already in use.");
        }

        if (batch.CourseId != request.CourseId)
        {
            var courseExists = await courseRepository.ExistsAsync(request.CourseId, cancellationToken);
            if (!courseExists)
                throw new NotFoundException(nameof(Course), request.CourseId);
        }

        if (IsTerminalStatus(batch.Status) && !IsTerminalStatus(request.Status))
            throw new ConflictException("Cannot reopen a completed or cancelled batch.");

        if (request.Status == BatchStatus.Active && batch.FacultyId == null && request.Status != batch.Status)
            throw new ConflictException("Cannot activate a batch without an assigned faculty.");

        if (batch.FacultyId.HasValue
            && (request.StartDate != batch.StartDate || request.EndDate != batch.EndDate)
            && request.Status != BatchStatus.Completed
            && request.Status != BatchStatus.Cancelled)
        {
            var facultyId = batch.FacultyId.Value;
            var conflicts = await batchRepository.FindAsync(
                b => b.Id != batch.Id
                    && b.FacultyId == facultyId
                    && b.Status != BatchStatus.Completed
                    && b.Status != BatchStatus.Cancelled
                    && b.StartDate <= request.EndDate
                    && request.StartDate <= b.EndDate,
                cancellationToken);

            if (conflicts.Count > 0)
                throw new ConflictException("Faculty already has another batch scheduled within this date range.");
        }

        batch.BatchCode = batchCode;
        batch.CourseId = request.CourseId;
        batch.StartDate = request.StartDate;
        batch.EndDate = request.EndDate;
        batch.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
        batch.MaxCapacity = request.MaxCapacity;
        batch.Status = request.Status;

        batchRepository.Update(batch);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private static bool IsTerminalStatus(BatchStatus status)
        => status is BatchStatus.Completed or BatchStatus.Cancelled;
}
