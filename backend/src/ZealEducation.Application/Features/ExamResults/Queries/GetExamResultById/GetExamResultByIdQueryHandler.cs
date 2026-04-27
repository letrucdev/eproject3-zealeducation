using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.ExamResults.Queries.GetExaminationResults;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.ExamResults.Queries.GetExamResultById;

public class GetExamResultByIdQueryHandler(
    IRepository<ExamResult> examResultRepository) : IRequestHandler<GetExamResultByIdQuery, ExamResultDto>
{
    public async Task<ExamResultDto> Handle(GetExamResultByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await examResultRepository.Query()
            .Where(r => r.Id == request.ResultId)
            .Select(r => new ExamResultDto
            {
                ResultId = r.Id,
                ExamId = r.ExamId,
                EnrollmentId = r.EnrollmentId,
                CandidateCode = r.Enrollment.Candidate.CandidateCode,
                CandidateFullName = r.Enrollment.Candidate.UserAccount.FullName,
                Score = r.Score,
                Grade = r.Grade,
                IsPassed = r.IsPassed,
                GradedById = r.GradedById,
                GradedByName = r.GradedBy.UserAccount.FullName,
                IsOverridden = r.IsOverridden,
                OverrideById = r.OverrideById,
                OverrideByName = r.OverrideBy != null ? r.OverrideBy.UserAccount.FullName : null,
                OverrideReason = r.OverrideReason,
                GradedAt = r.GradedAt,
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(ExamResult), request.ResultId);

        return dto;
    }
}
