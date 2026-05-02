using FluentValidation.TestHelper;
using ZealEducation.Application.Features.ClassSessions.Commands.MarkAttendance;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.UnitTests.Validators.ClassSessions;

public class MarkAttendanceCommandValidatorTests
{
    private readonly MarkAttendanceCommandValidator _validator = new();

    private static AttendanceEntry Entry(
        Guid? enrollmentId = null,
        AttendanceStatus status = AttendanceStatus.Present,
        decimal? practicalHours = null,
        string? remarks = null) => new(
            enrollmentId ?? Guid.NewGuid(),
            status,
            practicalHours,
            remarks);

    private static MarkAttendanceCommand Valid(
        Guid? sessionId = null,
        List<AttendanceEntry>? entries = null) => new(
            sessionId ?? Guid.NewGuid(),
            entries ?? [Entry()]);

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
    public void Should_fail_when_entries_list_is_empty()
    {
        var result = _validator.TestValidate(Valid(entries: []));
        result.ShouldHaveValidationErrorFor(c => c.Entries)
            .WithErrorMessage("At least one attendance entry is required.");
    }

    [Fact]
    public void Should_fail_when_entry_enrollment_id_is_empty()
    {
        var result = _validator.TestValidate(Valid(entries:
        [
            Entry(enrollmentId: Guid.Empty)
        ]));
        result.ShouldHaveValidationErrorFor("Entries[0].EnrollmentId");
    }

    [Fact]
    public void Should_fail_when_entry_status_is_not_in_enum()
    {
        var result = _validator.TestValidate(Valid(entries:
        [
            Entry(status: (AttendanceStatus)999)
        ]));
        result.ShouldHaveValidationErrorFor("Entries[0].Status");
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1000.00)]
    public void Should_fail_when_practical_hours_is_out_of_range(decimal hours)
    {
        var result = _validator.TestValidate(Valid(entries:
        [
            Entry(practicalHours: hours)
        ]));
        result.ShouldHaveValidationErrorFor("Entries[0].PracticalHours");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(50.5)]
    [InlineData(999.99)]
    public void Should_pass_when_practical_hours_is_in_range(decimal hours)
    {
        var result = _validator.TestValidate(Valid(entries:
        [
            Entry(practicalHours: hours)
        ]));
        result.ShouldNotHaveValidationErrorFor("Entries[0].PracticalHours");
    }

    [Fact]
    public void Should_pass_when_practical_hours_is_null()
    {
        var result = _validator.TestValidate(Valid(entries:
        [
            Entry(practicalHours: null)
        ]));
        result.ShouldNotHaveValidationErrorFor("Entries[0].PracticalHours");
    }

    [Fact]
    public void Should_fail_when_entry_remarks_exceeds_500_chars()
    {
        var result = _validator.TestValidate(Valid(entries:
        [
            Entry(remarks: new string('r', 501))
        ]));
        result.ShouldHaveValidationErrorFor("Entries[0].Remarks");
    }
}
