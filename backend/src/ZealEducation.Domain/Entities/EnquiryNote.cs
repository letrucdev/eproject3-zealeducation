using ZealEducation.Domain.Common;

namespace ZealEducation.Domain.Entities;

public class EnquiryNote : BaseAuditableEntity
{
    public Guid EnquiryId { get; set; }
    public Guid AuthorStaffId { get; set; }
    public string Content { get; set; } = default!;

    public CourseEnquiry Enquiry { get; set; } = default!;
    public Staff AuthorStaff { get; set; } = default!;
}
