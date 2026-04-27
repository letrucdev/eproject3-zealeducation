using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Batches.Queries.GetBatchById;

public class GetBatchByIdQueryHandler(
    IRepository<Batch> batchRepository) : IRequestHandler<GetBatchByIdQuery, BatchDetailDto>
{
    public async Task<BatchDetailDto> Handle(GetBatchByIdQuery request, CancellationToken cancellationToken)
    {
        var detail = await batchRepository.Query()
            .Where(b => b.Id == request.BatchId)
            .Select(b => new BatchDetailDto
            {
                BatchId = b.Id,
                BatchCode = b.BatchCode,
                CourseId = b.CourseId,
                CourseName = b.Course.CourseName,
                FacultyId = b.FacultyId,
                FacultyName = b.Faculty != null ? b.Faculty.Staff.UserAccount.FullName : null,
                FacultyCode = b.Faculty != null ? b.Faculty.FacultyCode : null,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                Location = b.Location,
                MaxCapacity = b.MaxCapacity,
                EnrolledCount = b.Enrollments.Count,
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return detail ?? throw new NotFoundException(nameof(Batch), request.BatchId);
    }
}
