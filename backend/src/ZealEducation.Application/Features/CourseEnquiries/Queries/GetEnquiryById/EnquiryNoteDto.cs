namespace ZealEducation.Application.Features.CourseEnquiries.Queries.GetEnquiryById;

public class EnquiryNoteDto
{
    public Guid NoteId { get; set; }
    public Guid AuthorStaffId { get; set; }
    public string AuthorFullName { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
