using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.CandidatePortal.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatches;

public class GetMyBatchesQueryHandler(
    ICurrentUser currentUser,
    IRepository<Candidate> candidateRepository,
    IRepository<Enrollment> enrollmentRepository) : IRequestHandler<GetMyBatchesQuery, PaginatedList<MyBatchListItemDto>>
{
    public async Task<PaginatedList<MyBatchListItemDto>> Handle(GetMyBatchesQuery request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateResolver.ResolveAsync(currentUser, candidateRepository, cancellationToken);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var baseQuery = enrollmentRepository.Query()
            .Where(e => e.CandidateId == candidate.Id && e.BatchId != null);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            baseQuery = baseQuery.Where(e =>
                EF.Functions.Like(e.Batch!.BatchCode, $"%{search}%") ||
                EF.Functions.Like(e.Course.CourseName, $"%{search}%"));
        }

        var sortKey = (request.SortBy ?? string.Empty).Trim().ToLower();
        var direction = (request.SortDirection ?? "desc").Trim().ToLower();

        var ordered = (sortKey, direction) switch
        {
            ("batchcode", "asc") => baseQuery.OrderBy(e => e.Batch!.BatchCode),
            ("batchcode", _) => baseQuery.OrderByDescending(e => e.Batch!.BatchCode),
            ("coursename", "asc") => baseQuery.OrderBy(e => e.Course.CourseName),
            ("coursename", _) => baseQuery.OrderByDescending(e => e.Course.CourseName),
            ("startdate", "asc") => baseQuery.OrderBy(e => e.Batch!.StartDate),
            ("startdate", _) => baseQuery.OrderByDescending(e => e.Batch!.StartDate),
            ("status", "asc") => baseQuery.OrderBy(e => e.Batch!.Status),
            ("status", _) => baseQuery.OrderByDescending(e => e.Batch!.Status),
            _ => baseQuery.OrderByDescending(e => e.Batch!.StartDate),
        };

        var projected = ordered.Select(e => new MyBatchListItemDto
        {
            EnrollmentId = e.Id,
            BatchId = e.Batch!.Id,
            BatchCode = e.Batch.BatchCode,
            CourseId = e.Course.Id,
            CourseName = e.Course.CourseName,
            FacultyId = e.Batch.FacultyId,
            FacultyName = e.Batch.Faculty != null ? e.Batch.Faculty.Staff.UserAccount.FullName : null,
            StartDate = e.Batch.StartDate,
            EndDate = e.Batch.EndDate,
            Location = e.Batch.Location,
            BatchStatus = e.Batch.Status,
            EnrollmentStatus = e.Status,
            EnrollmentDate = e.EnrollmentDate
        });

        return await PaginatedList<MyBatchListItemDto>.CreateAsync(projected, page, pageSize);
    }
}
