namespace ZealEducation.Application.Features.Candidates.Notifications;

public class CandidatePasswordResetEmailModel
{
    public string RecipientEmail { get; init; } = default!;
    public string RecipientName { get; init; } = default!;

    public string Username { get; init; } = default!;
    public string TemporaryPassword { get; init; } = default!;

    public string CandidateCode { get; init; } = default!;
    public DateTime ResetAt { get; init; }
}
