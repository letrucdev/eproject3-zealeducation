using MediatR;

namespace ZealEducation.Application.Features.CertificateApplications.Commands.RegenerateCertificate;

public record RegenerateCertificateCommand(Guid ApplicationId) : IRequest<RegenerateCertificateResponse>;

public record RegenerateCertificateResponse(Guid ApplicationId, string CertificateNumber);
