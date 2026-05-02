using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Batches.Commands.UpdateBatch;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.UnitTests.Validators.Batches;

public class UpdateBatchCommandValidatorTests
{
    private readonly UpdateBatchCommandValidator _validator = new();

    private static UpdateBatchCommand Valid(
        Guid? batchId = null,
        string batchCode = "B-001",
        Guid? courseId = null,
        DateOnly? start = null,
        DateOnly? end = null,
        string? location = "Room A",
        int maxCapacity = 30,
        BatchStatus status = BatchStatus.Active) => new(
            batchId ?? Guid.NewGuid(),
            batchCode,
            courseId ?? Guid.NewGuid(),
            start ?? new DateOnly(2030, 1, 1),
            end ?? new DateOnly(2030, 6, 1),
            location,
            maxCapacity,
            status);

    [Fact]
    public void Should_pass_for_valid_command()
    {
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_batch_id_is_empty()
    {
        _validator.TestValidate(Valid(batchId: Guid.Empty))
            .ShouldHaveValidationErrorFor(c => c.BatchId);
    }

    [Fact]
    public void Should_fail_when_status_is_not_in_enum()
    {
        var cmd = Valid(status: (BatchStatus)999);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.Status);
    }

    [Fact]
    public void Should_fail_when_end_date_equals_start_date()
    {
        var d = new DateOnly(2030, 1, 1);
        _validator.TestValidate(Valid(start: d, end: d))
            .ShouldHaveValidationErrorFor(c => c.EndDate);
    }
}
