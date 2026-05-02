using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CertificateApplications.Commands.RegenerateCertificate;

public class RegenerateCertificateCommandHandler(
    IRepository<CertificateApplication> applicationRepository,
    ICertificateArchiver archiver) : IRequestHandler<RegenerateCertificateCommand, RegenerateCertificateResponse>
{
    public async Task<RegenerateCertificateResponse> Handle(
        RegenerateCertificateCommand request, CancellationToken cancellationToken)
    {
        var application = await applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException(nameof(CertificateApplication), request.ApplicationId);

        if (application.Status != CertificateApplicationStatus.Approved
            || string.IsNullOrEmpty(application.CertificateNumber))
        {
            throw new ConflictException("Certificate has not been approved yet.");
        }

        await archiver.GenerateAndUploadAsync(application.Id, cancellationToken, forceRegenerate: true);

        return new RegenerateCertificateResponse(application.Id, application.CertificateNumber);
    }
}
