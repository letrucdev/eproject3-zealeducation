using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Batches.Queries.GetBatchById;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;
using FacultyEntity = ZealEducation.Domain.Entities.Faculty;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyBatchById;

public class GetFacultyBatchByIdQueryHandler(
    IRepository<Batch> batchRepository,
    IRepository<FacultyEntity> facultyRepository,
    ICurrentUser currentUser) : IRequestHandler<GetFacultyBatchByIdQuery, BatchDetailDto>
{
    public async Task<BatchDetailDto> Handle(GetFacultyBatchByIdQuery request, CancellationToken cancellationToken)
    {
        var facultyId = await facultyRepository.ResolveFacultyIdAsync(currentUser, cancellationToken);

        var detail = await batchRepository.Query()
            .Where(b => b.Id == request.BatchId && b.FacultyId == facultyId)
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
                UpdatedAt = b.UpdatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken);

        return detail ?? throw new NotFoundException(nameof(Batch), request.BatchId);
    }
}
