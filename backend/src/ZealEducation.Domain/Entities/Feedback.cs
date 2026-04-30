using ZealEducation.Domain.Common;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Domain.Entities;

public class Feedback : BaseAuditableEntity
{
    public Guid CandidateId { get; set; }
    public Guid BatchId { get; set; }
    public FeedbackType Type { get; set; }
    public Guid? TargetFacultyId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }

    public bool IsProcessed { get; set; }
    public Guid? ProcessedById { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public Candidate Candidate { get; set; } = default!;
    public Batch Batch { get; set; } = default!;
    public Faculty? TargetFaculty { get; set; }
    public UserAccount? ProcessedBy { get; set; }
}
