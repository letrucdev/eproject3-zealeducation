using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CandidatePortal.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchExamResults;

public class GetMyBatchExamResultsQueryHandler(
    ICurrentUser currentUser,
    IRepository<Candidate> candidateRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Examination> examinationRepository,
    IRepository<ExamResult> resultRepository) : IRequestHandler<GetMyBatchExamResultsQuery, MyBatchExamResultsDto>
{
    public async Task<MyBatchExamResultsDto> Handle(GetMyBatchExamResultsQuery request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateResolver.ResolveAsync(currentUser, candidateRepository, cancellationToken);

        var enrollment = await enrollmentRepository.Query()
            .Where(e => e.CandidateId == candidate.Id && e.BatchId == request.BatchId)
            .Select(e => new { e.Id })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("You are not enrolled in this batch.");

        var rows = await (
            from exam in examinationRepository.Query()
            where exam.BatchId == request.BatchId
            join r in resultRepository.Query().Where(x => x.EnrollmentId == enrollment.Id)
                on exam.Id equals r.ExamId into joined
            from r in joined.DefaultIfEmpty()
            orderby exam.ExamDate descending
            select new MyExamResultRowDto
            {
                ExamId = exam.Id,
                ExamName = exam.ExamName,
                ExamDate = exam.ExamDate,
                MaxScore = exam.MaxScore,
                PassScore = exam.PassScore,
                Location = exam.Location,
                ResultId = r != null ? (Guid?)r.Id : null,
                Score = r != null ? (decimal?)r.Score : null,
                Grade = r != null ? r.Grade : null,
                IsPassed = r != null ? (bool?)r.IsPassed : null,
                IsOverridden = r != null && r.IsOverridden,
                GradedAt = r != null ? (DateTime?)r.GradedAt : null
            }).ToListAsync(cancellationToken);

        return new MyBatchExamResultsDto
        {
            BatchId = request.BatchId,
            EnrollmentId = enrollment.Id,
            Rows = rows
        };
    }
}
