using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.ExamResults.Queries.GetExaminationResults;

public class GetExaminationResultsQueryHandler(
    IRepository<Examination> examinationRepository,
    IRepository<ExamResult> examResultRepository) : IRequestHandler<GetExaminationResultsQuery, PaginatedList<ExamResultDto>>
{
    public async Task<PaginatedList<ExamResultDto>> Handle(GetExaminationResultsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var examExists = await examinationRepository.ExistsAsync(request.ExaminationId, cancellationToken);
        if (!examExists)
            throw new NotFoundException(nameof(Examination), request.ExaminationId);

        var query = examResultRepository.Query().Where(r => r.ExamId == request.ExaminationId);

        var sortKey = (request.SortBy ?? string.Empty).Trim().ToLower();
        var direction = (request.SortDirection ?? "desc").Trim().ToLower();

        var ordered = (sortKey, direction) switch
        {
            ("score", "asc") => query.OrderBy(r => r.Score),
            ("score", _) => query.OrderByDescending(r => r.Score),
            ("candidatefullname", "desc") => query.OrderByDescending(r => r.Enrollment.Candidate.UserAccount.FullName),
            ("candidatefullname", _) => query.OrderBy(r => r.Enrollment.Candidate.UserAccount.FullName),
            ("gradedat", "asc") => query.OrderBy(r => r.GradedAt),
            _ => query.OrderByDescending(r => r.GradedAt),
        };

        var projected = ordered.Select(r => new ExamResultDto
        {
            ResultId = r.Id,
            ExamId = r.ExamId,
            EnrollmentId = r.EnrollmentId,
            CandidateCode = r.Enrollment.Candidate.CandidateCode,
            CandidateFullName = r.Enrollment.Candidate.UserAccount.FullName,
            Score = r.Score,
            Grade = r.Grade,
            IsPassed = r.IsPassed,
            IsFinalized = r.IsFinalized,
            GradedById = r.GradedById,
            GradedByName = r.GradedBy.UserAccount.FullName,
            IsOverridden = r.IsOverridden,
            OverrideById = r.OverrideById,
            OverrideByName = r.OverrideBy != null ? r.OverrideBy.UserAccount.FullName : null,
            OverrideReason = r.OverrideReason,
            GradedAt = r.GradedAt,
        });

        return await PaginatedList<ExamResultDto>.CreateAsync(projected, page, pageSize);
    }
}
