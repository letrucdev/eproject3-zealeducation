using MediatR;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;
using FacultyEntity = ZealEducation.Domain.Entities.Faculty;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyExaminations;

public class GetFacultyExaminationsQueryHandler(
    IRepository<Examination> examinationRepository,
    IRepository<FacultyEntity> facultyRepository,
    ICurrentUser currentUser) : IRequestHandler<GetFacultyExaminationsQuery, PaginatedList<FacultyExaminationSummaryDto>>
{
    public async Task<PaginatedList<FacultyExaminationSummaryDto>> Handle(GetFacultyExaminationsQuery request, CancellationToken cancellationToken)
    {
        var facultyId = await facultyRepository.ResolveFacultyIdAsync(currentUser, cancellationToken);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var query = examinationRepository.Query()
            .Where(e => e.Batch.FacultyId == facultyId && e.Batch.Status == BatchStatus.Active);

        if (request.BatchId.HasValue)
        {
            var batchId = request.BatchId.Value;
            query = query.Where(e => e.BatchId == batchId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(e =>
                e.ExamName.ToLower().Contains(search) ||
                (e.Location != null && e.Location.ToLower().Contains(search)));
        }

        var sortKey = (request.SortBy ?? string.Empty).Trim().ToLower();
        var direction = (request.SortDirection ?? "desc").Trim().ToLower();

        var ordered = (sortKey, direction) switch
        {
            ("examname", "desc") => query.OrderByDescending(e => e.ExamName),
            ("examname", _) => query.OrderBy(e => e.ExamName),
            ("batchcode", "desc") => query.OrderByDescending(e => e.Batch.BatchCode),
            ("batchcode", _) => query.OrderBy(e => e.Batch.BatchCode),
            ("examdate", "asc") => query.OrderBy(e => e.ExamDate),
            ("examdate", _) => query.OrderByDescending(e => e.ExamDate),
            ("maxscore", "asc") => query.OrderBy(e => e.MaxScore),
            ("maxscore", "desc") => query.OrderByDescending(e => e.MaxScore),
            _ => query.OrderByDescending(e => e.CreatedAt),
        };

        var projected = ordered.Select(e => new FacultyExaminationSummaryDto
        {
            ExaminationId = e.Id,
            BatchId = e.BatchId,
            BatchCode = e.Batch.BatchCode,
            CourseName = e.Batch.Course.CourseName,
            ExamName = e.ExamName,
            ExamDate = e.ExamDate,
            Location = e.Location,
            MaxScore = e.MaxScore,
            PassScore = e.PassScore,
            ResultCount = e.ExamResults.Count,
            AverageScore = e.ExamResults.Any() ? e.ExamResults.Average(r => r.Score) : (decimal?)null,
            MinScore = e.ExamResults.Any() ? e.ExamResults.Min(r => r.Score) : (decimal?)null,
            MaxStudentScore = e.ExamResults.Any() ? e.ExamResults.Max(r => r.Score) : (decimal?)null,
            CreatedAt = e.CreatedAt,
        });

        return await PaginatedList<FacultyExaminationSummaryDto>.CreateAsync(projected, page, pageSize);
    }
}
