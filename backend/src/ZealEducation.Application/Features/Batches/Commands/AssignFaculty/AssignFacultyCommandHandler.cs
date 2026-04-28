using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Batches.Commands.AssignFaculty;

public class AssignFacultyCommandHandler(
    IRepository<Batch> batchRepository,
    IRepository<Faculty> facultyRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AssignFacultyCommand, Unit>
{
    public async Task<Unit> Handle(AssignFacultyCommand request, CancellationToken cancellationToken)
    {
        var batch = await batchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Batch), request.BatchId);

        if (batch.Status is BatchStatus.Completed or BatchStatus.Cancelled)
            throw new ConflictException("Cannot assign faculty to a completed or cancelled batch.");

        if (request.FacultyId.HasValue)
        {
            var facultyExists = await facultyRepository.ExistsAsync(request.FacultyId.Value, cancellationToken);
            if (!facultyExists)
                throw new NotFoundException(nameof(Faculty), request.FacultyId.Value);

            var conflicts = await batchRepository.FindAsync(
                b => b.Id != batch.Id
                    && b.FacultyId == request.FacultyId.Value
                    && b.Status != BatchStatus.Completed
                    && b.Status != BatchStatus.Cancelled
                    && b.StartDate <= batch.EndDate
                    && batch.StartDate <= b.EndDate,
                cancellationToken);

            if (conflicts.Count > 0)
                throw new ConflictException("Faculty already has another batch scheduled within this date range.");
        }

        batch.FacultyId = request.FacultyId;

        if (request.FacultyId.HasValue)
        {
            if (batch.Status == BatchStatus.NeedsInstructor)
                batch.Status = BatchStatus.Active;
        }
        else
        {
            if (batch.Status == BatchStatus.Active)
                batch.Status = BatchStatus.NeedsInstructor;
        }

        batchRepository.Update(batch);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
