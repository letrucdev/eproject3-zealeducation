namespace ZealEducation.Application.Features.CourseEnquiries.Commands.AddEnquiryNote;

public class AddEnquiryNoteResponse
{
    public Guid NoteId { get; init; }
    public Guid EnquiryId { get; init; }
    public Guid AuthorStaffId { get; init; }
    public string AuthorFullName { get; init; } = default!;
    public string Content { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
}
