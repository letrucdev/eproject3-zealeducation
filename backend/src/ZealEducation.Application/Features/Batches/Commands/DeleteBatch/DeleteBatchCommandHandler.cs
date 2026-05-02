using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Batches.Commands.DeleteBatch;

public class DeleteBatchCommandHandler(
    IRepository<Batch> batchRepository,
    IRepository<Enrollment> enrollmentRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteBatchCommand, Unit>
{
    public async Task<Unit> Handle(DeleteBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await batchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Batch), request.BatchId);

        if (batch.FacultyId != null)
            throw new ConflictException("Cannot delete a batch that has an assigned faculty.");

        var enrollments = await enrollmentRepository.FindAsync(
            e => e.BatchId == batch.Id,
            cancellationToken);
        if (enrollments.Count > 0)
            throw new ConflictException("Cannot delete a batch that has enrollments.");

        batchRepository.Delete(batch);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
