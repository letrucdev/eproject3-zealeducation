using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;
using FacultyEntity = ZealEducation.Domain.Entities.Faculty;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyExaminationCandidates;

public class GetFacultyExaminationCandidatesQueryHandler(
    IRepository<Examination> examinationRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<FacultyEntity> facultyRepository,
    ICurrentUser currentUser) : IRequestHandler<GetFacultyExaminationCandidatesQuery, PaginatedList<FacultyExaminationCandidateDto>>
{
    public async Task<PaginatedList<FacultyExaminationCandidateDto>> Handle(GetFacultyExaminationCandidatesQuery request, CancellationToken cancellationToken)
    {
        var facultyId = await facultyRepository.ResolveFacultyIdAsync(currentUser, cancellationToken);

        var exam = await examinationRepository.Query()
            .Where(e => e.Id == request.ExaminationId && e.Batch.FacultyId == facultyId)
            .Select(e => new { e.Id, e.BatchId })
            .FirstOrDefaultAsync(cancellationToken);

        if (exam is null)
            throw new NotFoundException(nameof(Examination), request.ExaminationId);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var query = enrollmentRepository.Query()
            .Where(en => en.BatchId == exam.BatchId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(en =>
                en.Candidate.CandidateCode.ToLower().Contains(search) ||
                en.Candidate.UserAccount.FullName.ToLower().Contains(search));
        }

        var sortKey = (request.SortBy ?? string.Empty).Trim().ToLower();
        var direction = (request.SortDirection ?? "asc").Trim().ToLower();

        var ordered = (sortKey, direction) switch
        {
            ("candidatecode", "desc") => query.OrderByDescending(en => en.Candidate.CandidateCode),
            ("candidatecode", _) => query.OrderBy(en => en.Candidate.CandidateCode),
            ("score", "desc") => query
                .OrderByDescending(en => en.ExamResults
                    .Where(r => r.ExamId == exam.Id)
                    .Select(r => (decimal?)r.Score)
                    .FirstOrDefault())
                .ThenBy(en => en.Candidate.UserAccount.FullName),
            ("score", _) => query
                .OrderBy(en => en.ExamResults
                    .Where(r => r.ExamId == exam.Id)
                    .Select(r => (decimal?)r.Score)
                    .FirstOrDefault())
                .ThenBy(en => en.Candidate.UserAccount.FullName),
            ("gradedat", "desc") => query
                .OrderByDescending(en => en.ExamResults
                    .Where(r => r.ExamId == exam.Id)
                    .Select(r => (DateTime?)r.GradedAt)
                    .FirstOrDefault())
                .ThenBy(en => en.Candidate.UserAccount.FullName),
            ("gradedat", _) => query
                .OrderBy(en => en.ExamResults
                    .Where(r => r.ExamId == exam.Id)
                    .Select(r => (DateTime?)r.GradedAt)
                    .FirstOrDefault())
                .ThenBy(en => en.Candidate.UserAccount.FullName),
            ("candidatefullname", "desc") => query.OrderByDescending(en => en.Candidate.UserAccount.FullName),
            _ => query.OrderBy(en => en.Candidate.UserAccount.FullName),
        };

        var projected = ordered.Select(en => new FacultyExaminationCandidateDto
        {
            EnrollmentId = en.Id,
            CandidateId = en.CandidateId,
            CandidateCode = en.Candidate.CandidateCode,
            CandidateFullName = en.Candidate.UserAccount.FullName,
            ResultId = en.ExamResults
                .Where(r => r.ExamId == exam.Id)
                .Select(r => (Guid?)r.Id)
                .FirstOrDefault(),
            Score = en.ExamResults
                .Where(r => r.ExamId == exam.Id)
                .Select(r => (decimal?)r.Score)
                .FirstOrDefault(),
            Grade = en.ExamResults
                .Where(r => r.ExamId == exam.Id)
                .Select(r => r.Grade)
                .FirstOrDefault(),
            IsPassed = en.ExamResults
                .Where(r => r.ExamId == exam.Id)
                .Select(r => (bool?)r.IsPassed)
                .FirstOrDefault(),
            IsFinalized = en.ExamResults
                .Where(r => r.ExamId == exam.Id)
                .Select(r => r.IsFinalized)
                .FirstOrDefault(),
            IsOverridden = en.ExamResults
                .Where(r => r.ExamId == exam.Id)
                .Select(r => r.IsOverridden)
                .FirstOrDefault(),
            GradedAt = en.ExamResults
                .Where(r => r.ExamId == exam.Id)
                .Select(r => (DateTime?)r.GradedAt)
                .FirstOrDefault(),
        });

        return await PaginatedList<FacultyExaminationCandidateDto>.CreateAsync(projected, page, pageSize);
    }
}
