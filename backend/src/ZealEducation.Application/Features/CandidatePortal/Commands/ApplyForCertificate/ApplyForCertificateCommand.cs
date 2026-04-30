using MediatR;

namespace ZealEducation.Application.Features.CandidatePortal.Commands.ApplyForCertificate;

public record ApplyForCertificateCommand(Guid BatchId) : IRequest<Guid>;
