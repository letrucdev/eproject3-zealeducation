using ZealEducation.Domain.Common;

namespace ZealEducation.Domain.Entities;

public class Examination : BaseAuditableEntity
{
    public Guid BatchId { get; set; }
    public string ExamName { get; set; } = default!;
    public DateOnly ExamDate { get; set; }
    public string? Location { get; set; }
    public int MaxScore { get; set; } = 100;
    public int PassScore { get; set; } = 50;
    public Guid ScheduledById { get; set; }

    public Batch Batch { get; set; } = default!;
    public Staff ScheduledBy { get; set; } = default!;
    public ICollection<ExamResult> ExamResults { get; set; } = [];
}
