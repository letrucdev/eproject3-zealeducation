using FluentValidation;

namespace ZealEducation.Application.Features.CandidatePortal.Commands.ApplyForCertificate;

public class ApplyForCertificateCommandValidator : AbstractValidator<ApplyForCertificateCommand>
{
    public ApplyForCertificateCommandValidator()
    {
        RuleFor(x => x.BatchId).NotEmpty();
    }
}
