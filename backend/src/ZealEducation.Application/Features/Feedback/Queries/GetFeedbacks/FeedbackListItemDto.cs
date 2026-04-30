using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Feedback.Queries.GetFeedbacks;

public class FeedbackListItemDto
{
    public Guid FeedbackId { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateCode { get; set; } = default!;
    public string CandidateName { get; set; } = default!;
    public Guid BatchId { get; set; }
    public string BatchCode { get; set; } = default!;
    public string CourseName { get; set; } = default!;
    public FeedbackType Type { get; set; }
    public Guid? TargetFacultyId { get; set; }
    public string? TargetFacultyName { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsProcessed { get; set; }
    public Guid? ProcessedById { get; set; }
    public string? ProcessedByName { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
