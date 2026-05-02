using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Batches.Queries.GetBatchEnrollments;

public class GetBatchEnrollmentsQueryHandler(
    IRepository<Batch> batchRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository) : IRequestHandler<GetBatchEnrollmentsQuery, PaginatedList<BatchEnrollmentItemDto>>
{
    public async Task<PaginatedList<BatchEnrollmentItemDto>> Handle(GetBatchEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var batchExists = await batchRepository.ExistsAsync(request.BatchId, cancellationToken);
        if (!batchExists)
            throw new NotFoundException(nameof(Batch), request.BatchId);

        var query =
            from enrollment in enrollmentRepository.Query()
            join candidate in candidateRepository.Query() on enrollment.CandidateId equals candidate.Id
            join user in userRepository.Query() on candidate.UserAccountId equals user.Id
            where enrollment.BatchId == request.BatchId
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

        var sortKey = (request.SortBy ?? string.Empty).Trim().ToLower();
        var direction = (request.SortDirection ?? "asc").Trim().ToLower();

        var ordered = (sortKey, direction) switch
        {
            ("candidatecode", "desc") => query.OrderByDescending(x => x.candidate.CandidateCode),
            ("candidatecode", _) => query.OrderBy(x => x.candidate.CandidateCode),
            ("fullname", "desc") => query.OrderByDescending(x => x.user.FullName),
            ("fullname", _) => query.OrderBy(x => x.user.FullName),
            ("status", "desc") => query.OrderByDescending(x => x.enrollment.Status),
            ("status", _) => query.OrderBy(x => x.enrollment.Status),
            ("enrollmentdate", "asc") => query.OrderBy(x => x.enrollment.EnrollmentDate),
            _ => query.OrderByDescending(x => x.enrollment.EnrollmentDate),
        };

        var projected = ordered
            .ThenBy(x => x.candidate.CandidateCode)
            .Select(x => new BatchEnrollmentItemDto
            {
                EnrollmentId = x.enrollment.Id,
                CandidateId = x.candidate.Id,
                CandidateCode = x.candidate.CandidateCode,
                FullName = x.user.FullName,
                Email = x.user.Email,
                Phone = x.user.Phone,
                EnrollmentDate = x.enrollment.EnrollmentDate,
                Status = x.enrollment.Status
            });

        return await PaginatedList<BatchEnrollmentItemDto>.CreateAsync(projected, page, pageSize);
    }
}
