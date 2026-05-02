using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CandidatePortal.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyCertificates;

public class GetMyCertificatesQueryHandler(
    ICurrentUser currentUser,
    IRepository<Candidate> candidateRepository,
    IRepository<CertificateApplication> applicationRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Batch> batchRepository,
    IRepository<Course> courseRepository) : IRequestHandler<GetMyCertificatesQuery, List<MyCertificateListItemDto>>
{
    public async Task<List<MyCertificateListItemDto>> Handle(GetMyCertificatesQuery request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateResolver.ResolveAsync(currentUser, candidateRepository, cancellationToken);

        var query =
            from app in applicationRepository.Query()
            join e in enrollmentRepository.Query() on app.EnrollmentId equals e.Id
            where e.CandidateId == candidate.Id
            join co in courseRepository.Query() on e.CourseId equals co.Id
            from b in batchRepository.Query()
                .Where(batch => e.BatchId != null && batch.Id == e.BatchId)
                .DefaultIfEmpty()
            orderby app.CreatedAt descending
            select new MyCertificateListItemDto
            {
                ApplicationId = app.Id,
                BatchId = b == null ? null : b.Id,
                BatchCode = b == null ? null : b.BatchCode,
                CourseId = co.Id,
                CourseName = co.CourseName,
                Status = app.Status,
                CertificateNumber = app.CertificateNumber,
                AppliedAt = app.CreatedAt,
                ApprovedAt = app.ApprovedAt,
            };

        return await query.ToListAsync(cancellationToken);
    }
}
