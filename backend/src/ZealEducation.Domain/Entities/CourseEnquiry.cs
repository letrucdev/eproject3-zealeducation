using ZealEducation.Domain.Common;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Domain.Entities;

public class CourseEnquiry : BaseAuditableEntity
{
    public string FullName { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Email { get; set; }
    public Guid CourseInterestedId { get; set; }
    public EnquirySource Source { get; set; }
    public EnquiryStatus Status { get; set; } = EnquiryStatus.New;
    public DateOnly? NextFollowUpDate { get; set; }
    public Guid AssignedCounselorId { get; set; }
    public Guid? ConvertedCandidateId { get; set; }
    public DateTime? ConvertedAt { get; set; }

    public Course CourseInterested { get; set; } = default!;
    public Staff AssignedCounselor { get; set; } = default!;
    public Candidate? ConvertedCandidate { get; set; }
    public ICollection<EnquiryNote> Notes { get; set; } = new List<EnquiryNote>();
}
