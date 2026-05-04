using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Batches.Commands.CreateBatch;

namespace ZealEducation.Application.UnitTests.Validators.Batches;

public class CreateBatchCommandValidatorTests
{
    private readonly CreateBatchCommandValidator _validator = new();

    private static CreateBatchCommand Valid(
        string? batchCode = "B-001",
        Guid? courseId = null,
        DateOnly? start = null,
        DateOnly? end = null,
        string? location = "Room A",
        int maxCapacity = 30) => new(
            batchCode!,
            courseId ?? Guid.NewGuid(),
            start ?? DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(7)),
            end ?? DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(60)),
            location,
            maxCapacity);

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_batch_code_is_empty(string code)
    {
        var result = _validator.TestValidate(Valid(batchCode: code));
        result.ShouldHaveValidationErrorFor(c => c.BatchCode);
    }

    [Fact]
    public void Should_fail_when_batch_code_exceeds_30_chars()
    {
        var result = _validator.TestValidate(Valid(batchCode: new string('B', 31)));
        result.ShouldHaveValidationErrorFor(c => c.BatchCode);
    }

    [Fact]
    public void Should_fail_when_course_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(courseId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.CourseId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Should_fail_when_max_capacity_not_positive(int capacity)
    {
        var result = _validator.TestValidate(Valid(maxCapacity: capacity));
        result.ShouldHaveValidationErrorFor(c => c.MaxCapacity);
    }

    [Fact]
    public void Should_fail_when_max_capacity_is_above_500()
    {
        var result = _validator.TestValidate(Valid(maxCapacity: 501));
        result.ShouldHaveValidationErrorFor(c => c.MaxCapacity);
    }

    [Fact]
    public void Should_fail_when_end_date_is_not_after_start_date()
    {
        var start = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(10));
        var result = _validator.TestValidate(Valid(start: start, end: start));
        result.ShouldHaveValidationErrorFor(c => c.EndDate);
    }

    [Fact]
    public void Should_fail_when_start_date_is_in_the_past()
    {
        var past = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1));
        var result = _validator.TestValidate(Valid(start: past, end: past.AddDays(30)));
        result.ShouldHaveValidationErrorFor(c => c.StartDate);
    }

    [Fact]
    public void Should_fail_when_location_exceeds_100_chars()
    {
        var result = _validator.TestValidate(Valid(location: new string('x', 101)));
        result.ShouldHaveValidationErrorFor(c => c.Location);
    }
}
