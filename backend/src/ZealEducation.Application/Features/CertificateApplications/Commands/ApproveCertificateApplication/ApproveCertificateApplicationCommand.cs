using MediatR;

namespace ZealEducation.Application.Features.CertificateApplications.Commands.ApproveCertificateApplication;

public record ApproveCertificateApplicationCommand(Guid ApplicationId) : IRequest<ApproveCertificateApplicationResponse>;

public record ApproveCertificateApplicationResponse(Guid ApplicationId, string CertificateNumber);
