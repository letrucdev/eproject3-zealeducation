using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Batches.Queries.GetBatches;

public class GetBatchesQueryHandler(
    IRepository<Batch> batchRepository) : IRequestHandler<GetBatchesQuery, PaginatedList<BatchListItemDto>>
{
    public async Task<PaginatedList<BatchListItemDto>> Handle(GetBatchesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var query = batchRepository.Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(b =>
                b.BatchCode.ToLower().Contains(search) ||
                (b.Location != null && b.Location.ToLower().Contains(search)));
        }

        if (request.CourseId.HasValue)
        {
            var courseId = request.CourseId.Value;
            query = query.Where(b => b.CourseId == courseId);
        }

        if (request.FacultyId.HasValue)
        {
            var facultyId = request.FacultyId.Value;
            query = query.Where(b => b.FacultyId == facultyId);
        }

        if (request.Status.HasValue)
        {
            var status = request.Status.Value;
            query = query.Where(b => b.Status == status);
        }

        var projected = query
            .OrderByDescending(b => b.CreatedAt)
            .ThenBy(b => b.BatchCode)
            .Select(b => new BatchListItemDto
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
            });

        return await PaginatedList<BatchListItemDto>.CreateAsync(projected, page, pageSize);
    }
}
