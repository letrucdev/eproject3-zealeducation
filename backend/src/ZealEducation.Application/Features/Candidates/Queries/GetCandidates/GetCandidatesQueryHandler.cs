using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;
using MediatR;

namespace ZealEducation.Application.Features.Candidates.Queries.GetCandidates;

public class GetCandidatesQueryHandler(
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Course> courseRepository,
    IRepository<Batch> batchRepository) : IRequestHandler<GetCandidatesQuery, PaginatedList<CandidateListItemDto>>
{
    public async Task<PaginatedList<CandidateListItemDto>> Handle(GetCandidatesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var query =
            from candidate in candidateRepository.Query()
            join user in userRepository.Query() on candidate.UserAccountId equals user.Id
            select new { candidate, user };

        if (request.Status.HasValue)
        {
            var status = request.Status.Value;
            query = query.Where(x => x.candidate.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.candidate.CandidateCode.ToLower().Contains(search) ||
                x.user.FullName.ToLower().Contains(search) ||
                x.user.Email.ToLower().Contains(search) ||
                x.user.Phone.Contains(search));
        }

        if (request.CourseId.HasValue)
        {
            var courseId = request.CourseId.Value;
            query = query.Where(x =>
                enrollmentRepository.Query().Any(e => e.CandidateId == x.candidate.Id && e.CourseId == courseId));
        }

        if (request.BatchId.HasValue)
        {
            var batchId = request.BatchId.Value;
            query = query.Where(x =>
                enrollmentRepository.Query().Any(e => e.CandidateId == x.candidate.Id && e.BatchId == batchId));
        }

        var projected = query
            .OrderByDescending(x => x.candidate.RegisteredAt)
            .ThenBy(x => x.candidate.CandidateCode)
            .Select(x => new
            {
                x.candidate,
                x.user,
                CurrentEnrollment = enrollmentRepository.Query()
                    .Where(e => e.CandidateId == x.candidate.Id && e.Status == EnrollmentStatus.Enrolled)
                    .OrderByDescending(e => e.EnrollmentDate)
                    .FirstOrDefault()
            })
            .Select(x => new CandidateListItemDto
            {
                CandidateId = x.candidate.Id,
                CandidateCode = x.candidate.CandidateCode,
                FullName = x.user.FullName,
                Email = x.user.Email,
                Phone = x.user.Phone,
                IsActive = x.user.IsActive,
                Status = x.candidate.Status,
                RegisteredAt = x.candidate.RegisteredAt,
                CurrentEnrollmentId = x.CurrentEnrollment != null ? x.CurrentEnrollment.Id : (Guid?)null,
                CurrentCourseId = x.CurrentEnrollment != null ? x.CurrentEnrollment.CourseId : (Guid?)null,
                CurrentCourseName = x.CurrentEnrollment != null
                    ? courseRepository.Query().Where(c => c.Id == x.CurrentEnrollment.CourseId).Select(c => c.CourseName).FirstOrDefault()
                    : null,
                CurrentBatchId = x.CurrentEnrollment != null ? x.CurrentEnrollment.BatchId : null,
                CurrentBatchCode = x.CurrentEnrollment != null && x.CurrentEnrollment.BatchId != null
                    ? batchRepository.Query().Where(b => b.Id == x.CurrentEnrollment.BatchId).Select(b => b.BatchCode).FirstOrDefault()
                    : null
            });

        return await PaginatedList<CandidateListItemDto>.CreateAsync(projected, page, pageSize);
    }
}
