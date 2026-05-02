namespace ZealEducation.Application.Features.CourseEnquiries.Commands.CreateEnquiry;

public class CreateEnquiryResponse
{
    public Guid EnquiryId { get; init; }
    public string FullName { get; init; } = default!;
    public string Phone { get; init; } = default!;
    public Guid CourseInterestedId { get; init; }
    public string CourseInterestedName { get; init; } = default!;
}
