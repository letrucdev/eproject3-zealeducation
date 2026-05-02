using MediatR;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyCertificateFile;

public record GetMyCertificateFileQuery(Guid ApplicationId) : IRequest<CertificateFileResult>;

public record CertificateFileResult(byte[] Content, string ContentType, string FileName);
