using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Examinations.Queries.GetBatchExaminations;

public class GetBatchExaminationsQueryHandler(
    IRepository<Batch> batchRepository,
    IRepository<Examination> examinationRepository) : IRequestHandler<GetBatchExaminationsQuery, PaginatedList<ExaminationDto>>
{
    public async Task<PaginatedList<ExaminationDto>> Handle(GetBatchExaminationsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var batchExists = await batchRepository.ExistsAsync(request.BatchId, cancellationToken);
        if (!batchExists)
            throw new NotFoundException(nameof(Batch), request.BatchId);

        var query = examinationRepository.Query().Where(e => e.BatchId == request.BatchId);

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
            ("location", "desc") => query.OrderByDescending(e => e.Location),
            ("location", _) => query.OrderBy(e => e.Location),
            ("maxscore", "desc") => query.OrderByDescending(e => e.MaxScore),
            ("maxscore", _) => query.OrderBy(e => e.MaxScore),
            ("passscore", "desc") => query.OrderByDescending(e => e.PassScore),
            ("passscore", _) => query.OrderBy(e => e.PassScore),
            ("createdat", "asc") => query.OrderBy(e => e.CreatedAt),
            ("createdat", _) => query.OrderByDescending(e => e.CreatedAt),
            ("examdate", "asc") => query.OrderBy(e => e.ExamDate),
            _ => query.OrderByDescending(e => e.ExamDate),
        };

        var projected = ordered
            .ThenByDescending(e => e.CreatedAt)
            .Select(e => new ExaminationDto
            {
                ExaminationId = e.Id,
                BatchId = e.BatchId,
                ExamName = e.ExamName,
                ExamDate = e.ExamDate,
                Location = e.Location,
                MaxScore = e.MaxScore,
                PassScore = e.PassScore,
                ScheduledById = e.ScheduledById,
                ScheduledByName = e.ScheduledBy.UserAccount.FullName,
                HasResults = e.ExamResults.Any(),
                CreatedAt = e.CreatedAt,
            });

        return await PaginatedList<ExaminationDto>.CreateAsync(projected, page, pageSize);
    }
}
