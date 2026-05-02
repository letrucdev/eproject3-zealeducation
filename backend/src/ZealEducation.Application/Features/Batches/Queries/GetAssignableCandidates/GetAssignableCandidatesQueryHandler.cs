using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Batches.Queries.GetAssignableCandidates;

public class GetAssignableCandidatesQueryHandler(
    IRepository<Batch> batchRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository) : IRequestHandler<GetAssignableCandidatesQuery, PaginatedList<AssignableCandidateDto>>
{
    public async Task<PaginatedList<AssignableCandidateDto>> Handle(GetAssignableCandidatesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var batch = await batchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Batch), request.BatchId);

        var query =
            from enrollment in enrollmentRepository.Query()
            join candidate in candidateRepository.Query() on enrollment.CandidateId equals candidate.Id
            join user in userRepository.Query() on candidate.UserAccountId equals user.Id
            where enrollment.CourseId == batch.CourseId
                && enrollment.BatchId == null
                && enrollment.Status == EnrollmentStatus.PendingAssignment
            select new { enrollment, candidate, user };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.candidate.CandidateCode.ToLower().Contains(search) ||
                x.user.FullName.ToLower().Contains(search) ||
                x.user.Email.ToLower().Contains(search) ||
                x.user.Phone.Contains(search));
        }

        var projected = query
            .OrderByDescending(x => x.enrollment.EnrollmentDate)
            .ThenBy(x => x.candidate.CandidateCode)
            .Select(x => new AssignableCandidateDto
            {
                EnrollmentId = x.enrollment.Id,
                CandidateId = x.candidate.Id,
                CandidateCode = x.candidate.CandidateCode,
                FullName = x.user.FullName,
                Email = x.user.Email,
                Phone = x.user.Phone,
                EnrollmentDate = x.enrollment.EnrollmentDate
            });

        return await PaginatedList<AssignableCandidateDto>.CreateAsync(projected, page, pageSize);
    }
}
