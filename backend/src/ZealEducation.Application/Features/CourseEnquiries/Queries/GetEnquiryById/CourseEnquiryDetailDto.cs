using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.CourseEnquiries.Queries.GetEnquiryById;

public class CourseEnquiryDetailDto
{
    public Guid EnquiryId { get; set; }
    public string FullName { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Email { get; set; }
    public Guid CourseInterestedId { get; set; }
    public string CourseInterestedName { get; set; } = default!;
    public EnquirySource Source { get; set; }
    public EnquiryStatus Status { get; set; }
    public DateOnly? NextFollowUpDate { get; set; }
    public Guid AssignedCounselorId { get; set; }
    public string AssignedCounselorName { get; set; } = default!;
    public Guid? ConvertedCandidateId { get; set; }
    public string? ConvertedCandidateCode { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<EnquiryNoteDto> Notes { get; set; } = [];
}
