using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;
using FacultyEntity = ZealEducation.Domain.Entities.Faculty;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyExaminationCandidates;

public class GetFacultyExaminationCandidatesQueryHandler(
    IRepository<Examination> examinationRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<FacultyEntity> facultyRepository,
    ICurrentUser currentUser) : IRequestHandler<GetFacultyExaminationCandidatesQuery, List<FacultyExaminationCandidateDto>>
{
    public async Task<List<FacultyExaminationCandidateDto>> Handle(GetFacultyExaminationCandidatesQuery request, CancellationToken cancellationToken)
    {
        var facultyId = await facultyRepository.ResolveFacultyIdAsync(currentUser, cancellationToken);

        var exam = await examinationRepository.Query()
            .Where(e => e.Id == request.ExaminationId && e.Batch.FacultyId == facultyId)
            .Select(e => new { e.Id, e.BatchId })
            .FirstOrDefaultAsync(cancellationToken);

        if (exam is null)
            throw new NotFoundException(nameof(Examination), request.ExaminationId);

        return await enrollmentRepository.Query()
            .Where(en => en.BatchId == exam.BatchId)
            .OrderBy(en => en.Candidate.UserAccount.FullName)
            .Select(en => new FacultyExaminationCandidateDto
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
            })
            .ToListAsync(cancellationToken);
    }
}
