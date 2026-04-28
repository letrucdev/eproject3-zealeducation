using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Batches.Commands.AssignCandidatesToBatch;

public class AssignCandidatesToBatchCommandHandler(
    IRepository<Batch> batchRepository,
    IRepository<Enrollment> enrollmentRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AssignCandidatesToBatchCommand, Unit>
{
    public async Task<Unit> Handle(AssignCandidatesToBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await batchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Batch), request.BatchId);

        if (batch.Status is BatchStatus.Completed or BatchStatus.Cancelled)
            throw new ConflictException("Cannot assign candidates to a completed or cancelled batch.");

        var enrollmentIds = request.EnrollmentIds.Distinct().ToList();

        var currentEnrolledCount = await enrollmentRepository.Query()
            .CountAsync(e => e.BatchId == batch.Id, cancellationToken);

        if (currentEnrolledCount + enrollmentIds.Count > batch.MaxCapacity)
            throw new ConflictException(
                $"Adding {enrollmentIds.Count} candidate(s) would exceed batch capacity ({batch.MaxCapacity}). Currently enrolled: {currentEnrolledCount}.");

        var enrollments = await enrollmentRepository.Query()
            .Where(e => enrollmentIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        if (enrollments.Count != enrollmentIds.Count)
            throw new NotFoundException("One or more enrollments were not found.");

        foreach (var enrollment in enrollments)
        {
            if (enrollment.CourseId != batch.CourseId)
                throw new ConflictException(
                    $"Enrollment {enrollment.Id} belongs to a different course and cannot be assigned to this batch.");

            if (enrollment.BatchId != null)
                throw new ConflictException(
                    $"Enrollment {enrollment.Id} is already assigned to a batch.");

            if (enrollment.Status != EnrollmentStatus.PendingAssignment)
                throw new ConflictException(
                    $"Enrollment {enrollment.Id} is not in pending assignment state.");

            enrollment.BatchId = batch.Id;
            enrollment.Status = EnrollmentStatus.Enrolled;
            enrollmentRepository.Update(enrollment);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
