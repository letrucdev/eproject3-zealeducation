namespace ZealEducation.Application.Features.CourseEnquiries.Commands.ConvertEnquiry;

public class ConvertEnquiryResponse
{
    public Guid CandidateId { get; init; }
    public Guid UserAccountId { get; init; }
    public string CandidateCode { get; init; } = default!;
    public string Username { get; init; } = default!;
    public string TemporaryPassword { get; init; } = default!;
    public string Email { get; init; } = default!;
}
