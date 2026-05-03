namespace ZealEducation.Application.Features.Batches.Notifications;

public class BatchCandidateEnrolledEmailModel
{
    public string RecipientEmail { get; init; } = default!;
    public string RecipientName { get; init; } = default!;

    public string CandidateCode { get; init; } = default!;

    public string BatchCode { get; init; } = default!;
    public string CourseName { get; init; } = default!;
    public DateOnly BatchStartDate { get; init; }
    public DateOnly BatchEndDate { get; init; }
    public string? BatchLocation { get; init; }

    public IReadOnlyList<BatchScheduleSession> Sessions { get; init; } = [];
}
