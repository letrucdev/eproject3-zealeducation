namespace ZealEducation.Application.Features.Examinations.Notifications;

public class ExaminationCreatedEmailModel
{
    public string RecipientEmail { get; init; } = default!;
    public string RecipientName { get; init; } = default!;

    public string BatchCode { get; init; } = default!;
    public string CourseName { get; init; } = default!;

    public string ExamName { get; init; } = default!;
    public DateOnly ExamDate { get; init; }
    public string? Location { get; init; }
    public int MaxScore { get; init; }
    public int PassScore { get; init; }
}
