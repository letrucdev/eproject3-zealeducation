namespace ZealEducation.Application.Features.Batches.Notifications;

public class BatchFacultyAssignedEmailModel
{
    public string RecipientEmail { get; init; } = default!;
    public string RecipientName { get; init; } = default!;

    public string FacultyCode { get; init; } = default!;

    public string BatchCode { get; init; } = default!;
    public string CourseName { get; init; } = default!;
    public DateOnly BatchStartDate { get; init; }
    public DateOnly BatchEndDate { get; init; }
    public string? BatchLocation { get; init; }

    public IReadOnlyList<BatchScheduleSession> Sessions { get; init; } = [];
}
