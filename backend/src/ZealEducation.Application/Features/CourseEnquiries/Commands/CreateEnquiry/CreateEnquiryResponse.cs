namespace ZealEducation.Application.Features.CourseEnquiries.Commands.CreateEnquiry;

public class CreateEnquiryResponse
{
    public Guid EnquiryId { get; init; }
    public string FullName { get; init; } = default!;
    public string Phone { get; init; } = default!;
    public string CourseInterested { get; init; } = default!;
}
