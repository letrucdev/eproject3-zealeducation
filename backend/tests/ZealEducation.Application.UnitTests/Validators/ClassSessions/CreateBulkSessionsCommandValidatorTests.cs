using FluentValidation.TestHelper;
using ZealEducation.Application.Features.ClassSessions.Commands.CreateBulkSessions;

namespace ZealEducation.Application.UnitTests.Validators.ClassSessions;

public class CreateBulkSessionsCommandValidatorTests
{
    private readonly CreateBulkSessionsCommandValidator _validator = new();

    private static CreateBulkSessionsCommand Valid(
        Guid? batchId = null,
        IReadOnlyList<DayOfWeek>? days = null,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        string? topic = "Intro",
        string? location = "Room A") => new(
            batchId ?? Guid.NewGuid(),
            days ?? new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday },
            startTime ?? new TimeOnly(9, 0),
            endTime ?? new TimeOnly(11, 0),
            topic,
            location);

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

    [Fact]
    public void Should_fail_when_days_of_week_is_empty()
    {
        var result = _validator.TestValidate(Valid(days: new List<DayOfWeek>()));
        result.ShouldHaveValidationErrorFor(c => c.DaysOfWeek)
            .WithErrorMessage("Select at least one day of the week.");
    }

    [Fact]
    public void Should_fail_when_days_of_week_has_duplicates()
    {
        var result = _validator.TestValidate(Valid(days:
            new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Monday }));
        result.ShouldHaveValidationErrorFor(c => c.DaysOfWeek)
            .WithErrorMessage("Days of week must not repeat.");
    }

    [Fact]
    public void Should_fail_when_a_day_is_not_in_enum()
    {
        var result = _validator.TestValidate(Valid(days:
            new List<DayOfWeek> { (DayOfWeek)99 }));
        result.ShouldHaveValidationErrorFor("DaysOfWeek[0]");
    }

    [Fact]
    public void Should_fail_when_end_time_is_not_after_start_time()
    {
        var t = new TimeOnly(10, 0);
        var result = _validator.TestValidate(Valid(startTime: t, endTime: t));
        result.ShouldHaveValidationErrorFor(c => c.EndTime)
            .WithErrorMessage("End time must be after start time.");
    }

    [Fact]
    public void Should_fail_when_topic_exceeds_200_chars()
    {
        var result = _validator.TestValidate(Valid(topic: new string('t', 201)));
        result.ShouldHaveValidationErrorFor(c => c.Topic);
    }

    [Fact]
    public void Should_fail_when_location_exceeds_100_chars()
    {
        var result = _validator.TestValidate(Valid(location: new string('x', 101)));
        result.ShouldHaveValidationErrorFor(c => c.Location);
    }
}
