using FluentValidation.TestHelper;
using ZealEducation.Application.Features.CandidatePortal.Commands.ApplyForCertificate;

namespace ZealEducation.Application.UnitTests.Validators.CandidatePortal;

public class ApplyForCertificateCommandValidatorTests
{
    private readonly ApplyForCertificateCommandValidator _validator = new();

    private static ApplyForCertificateCommand Valid(Guid? batchId = null) =>
        new(batchId ?? Guid.NewGuid());

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_batch_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(batchId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.BatchId);
    }
}
