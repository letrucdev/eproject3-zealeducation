using FluentValidation.TestHelper;
using ZealEducation.Application.Features.ClassSessions.Commands.CreateClassSession;

namespace ZealEducation.Application.UnitTests.Validators.ClassSessions;

public class CreateClassSessionCommandValidatorTests
{
    private readonly CreateClassSessionCommandValidator _validator = new();

    private static CreateClassSessionCommand Valid(
        Guid? batchId = null,
        DateOnly? sessionDate = null,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        string? topic = "Intro",
        string? location = "Room A") => new(
            batchId ?? Guid.NewGuid(),
            sessionDate ?? new DateOnly(2030, 1, 15),
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
    public void Should_fail_when_end_time_is_equal_to_start_time()
    {
        var t = new TimeOnly(10, 0);
        var result = _validator.TestValidate(Valid(startTime: t, endTime: t));
        result.ShouldHaveValidationErrorFor(c => c.EndTime)
            .WithErrorMessage("End time must be after start time.");
    }

    [Fact]
    public void Should_fail_when_end_time_is_before_start_time()
    {
        var result = _validator.TestValidate(Valid(
            startTime: new TimeOnly(12, 0),
            endTime: new TimeOnly(11, 0)));
        result.ShouldHaveValidationErrorFor(c => c.EndTime);
    }

    [Fact]
    public void Should_fail_when_topic_exceeds_200_chars()
    {
        var result = _validator.TestValidate(Valid(topic: new string('t', 201)));
        result.ShouldHaveValidationErrorFor(c => c.Topic);
    }

    [Fact]
    public void Should_pass_when_topic_is_null()
    {
        var result = _validator.TestValidate(Valid(topic: null));
        result.ShouldNotHaveValidationErrorFor(c => c.Topic);
    }

    [Fact]
    public void Should_fail_when_location_exceeds_100_chars()
    {
        var result = _validator.TestValidate(Valid(location: new string('x', 101)));
        result.ShouldHaveValidationErrorFor(c => c.Location);
    }

    [Fact]
    public void Should_pass_when_location_is_null()
    {
        var result = _validator.TestValidate(Valid(location: null));
        result.ShouldNotHaveValidationErrorFor(c => c.Location);
    }
}
