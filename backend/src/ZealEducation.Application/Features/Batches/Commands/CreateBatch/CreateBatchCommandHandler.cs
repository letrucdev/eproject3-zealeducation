using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Batches.Commands.CreateBatch;

public class CreateBatchCommandHandler(
    IRepository<Batch> batchRepository,
    IRepository<Course> courseRepository,
    IRepository<Faculty> facultyRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBatchCommand, CreateBatchResponse>
{
    public async Task<CreateBatchResponse> Handle(CreateBatchCommand request, CancellationToken cancellationToken)
    {
        var batchCode = request.BatchCode.Trim();

        var courseExists = await courseRepository.ExistsAsync(request.CourseId, cancellationToken);
        if (!courseExists)
            throw new NotFoundException(nameof(Course), request.CourseId);

        if (request.FacultyId.HasValue)
        {
            var facultyExists = await facultyRepository.ExistsAsync(request.FacultyId.Value, cancellationToken);
            if (!facultyExists)
                throw new NotFoundException(nameof(Faculty), request.FacultyId.Value);

            var conflicts = await batchRepository.FindAsync(
                b => b.FacultyId == request.FacultyId.Value
                    && b.Status != BatchStatus.Completed
                    && b.Status != BatchStatus.Cancelled
                    && b.StartDate <= request.EndDate
                    && request.StartDate <= b.EndDate,
                cancellationToken);

            if (conflicts.Count > 0)
                throw new ConflictException("Faculty already has another batch scheduled within this date range.");
        }

        var duplicates = await batchRepository.FindAsync(b => b.BatchCode == batchCode, cancellationToken);
        if (duplicates.Count > 0)
            throw new ConflictException("Batch code is already in use.");

        var batch = new Batch
        {
            Id = Guid.NewGuid(),
            BatchCode = batchCode,
            CourseId = request.CourseId,
            FacultyId = request.FacultyId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
            MaxCapacity = request.MaxCapacity,
            Status = request.FacultyId.HasValue ? BatchStatus.Active : BatchStatus.NeedsInstructor
        };

        await batchRepository.AddAsync(batch, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateBatchResponse
        {
            BatchId = batch.Id,
            BatchCode = batch.BatchCode,
            Status = batch.Status
        };
    }
}
