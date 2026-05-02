using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Batches.Commands.AssignCandidatesToBatch;

namespace ZealEducation.Application.UnitTests.Validators.Batches;

public class AssignCandidatesToBatchCommandValidatorTests
{
    private readonly AssignCandidatesToBatchCommandValidator _validator = new();

    [Fact]
    public void Should_pass_with_valid_input()
    {
        var cmd = new AssignCandidatesToBatchCommand(Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid()]);
        _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_batch_id_is_empty()
    {
        var cmd = new AssignCandidatesToBatchCommand(Guid.Empty, [Guid.NewGuid()]);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.BatchId);
    }

    [Fact]
    public void Should_fail_when_enrollment_ids_list_is_empty()
    {
        var cmd = new AssignCandidatesToBatchCommand(Guid.NewGuid(), []);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.EnrollmentIds);
    }

    [Fact]
    public void Should_fail_when_any_enrollment_id_is_empty_guid()
    {
        var cmd = new AssignCandidatesToBatchCommand(Guid.NewGuid(), [Guid.NewGuid(), Guid.Empty]);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor("EnrollmentIds[1]");
    }
}
