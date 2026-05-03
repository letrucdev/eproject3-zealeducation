using FluentValidation.TestHelper;
using ZealEducation.Application.Features.ClassSessions.Commands.UpdateClassSession;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.UnitTests.Validators.ClassSessions;

public class UpdateClassSessionCommandValidatorTests
{
    private readonly UpdateClassSessionCommandValidator _validator = new();

    private static UpdateClassSessionCommand Valid(
        Guid? sessionId = null,
        DateOnly? sessionDate = null,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        string? topic = "Intro",
        string? location = "Room A",
        ClassSessionStatus status = ClassSessionStatus.Scheduled) => new(
            sessionId ?? Guid.NewGuid(),
            sessionDate ?? new DateOnly(2030, 1, 15),
            startTime ?? new TimeOnly(9, 0),
            endTime ?? new TimeOnly(11, 0),
            topic,
            location,
            status);

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_session_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(sessionId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.SessionId);
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

    [Fact]
    public void Should_fail_when_status_is_not_in_enum()
    {
        var result = _validator.TestValidate(Valid(status: (ClassSessionStatus)999));
        result.ShouldHaveValidationErrorFor(c => c.Status);
    }

    [Theory]
    [InlineData(ClassSessionStatus.Scheduled)]
    [InlineData(ClassSessionStatus.Completed)]
    [InlineData(ClassSessionStatus.Cancelled)]
    public void Should_pass_when_status_is_valid_enum(ClassSessionStatus status)
    {
        var result = _validator.TestValidate(Valid(status: status));
        result.ShouldNotHaveValidationErrorFor(c => c.Status);
    }
}
