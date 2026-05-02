using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Batches.Commands.DeleteBatch;

namespace ZealEducation.Application.UnitTests.Validators.Batches;

public class DeleteBatchCommandValidatorTests
{
    private readonly DeleteBatchCommandValidator _validator = new();

    [Fact]
    public void Should_pass_for_valid_command()
    {
        var cmd = new DeleteBatchCommand(Guid.NewGuid());
        _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_batch_id_is_empty()
    {
        var cmd = new DeleteBatchCommand(Guid.Empty);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.BatchId);
    }
}
