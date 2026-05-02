namespace ZealEducation.Application.Features.Candidates.Commands.ResetCandidatePassword;

public class ResetCandidatePasswordResponse
{
    public string Username { get; set; } = default!;
    public string TemporaryPassword { get; set; } = default!;
}
