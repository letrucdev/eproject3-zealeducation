using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CandidatePortal.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyCertificateFile;

public class GetMyCertificateFileQueryHandler(
    ICurrentUser currentUser,
    IRepository<Candidate> candidateRepository,
    IRepository<CertificateApplication> applicationRepository,
    IRepository<Enrollment> enrollmentRepository,
    ICertificateArchiver archiver) : IRequestHandler<GetMyCertificateFileQuery, CertificateFileResult>
{
    public async Task<CertificateFileResult> Handle(GetMyCertificateFileQuery request, CancellationToken cancellationToken)
    {
        var candidate = await CandidateResolver.ResolveAsync(currentUser, candidateRepository, cancellationToken);

        var application = await applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException(nameof(CertificateApplication), request.ApplicationId);

        var enrollment = await enrollmentRepository.GetByIdAsync(application.EnrollmentId, cancellationToken);
        if (enrollment is null || enrollment.CandidateId != candidate.Id)
            throw new NotFoundException(nameof(CertificateApplication), request.ApplicationId);

        if (application.Status != CertificateApplicationStatus.Approved
            || string.IsNullOrEmpty(application.CertificateNumber))
        {
            throw new ConflictException("Certificate has not been approved yet.");
        }

        var bytes = await archiver.GenerateAndUploadAsync(application.Id, cancellationToken);
        var fileName = $"{application.CertificateNumber}.pdf";
        return new CertificateFileResult(bytes, "application/pdf", fileName);
    }
}
