using ZealEducation.Domain.Common;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Domain.Entities;

public class ClassSession : BaseAuditableEntity
{
    public Guid BatchId { get; set; }
    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Topic { get; set; }
    public string? Location { get; set; }
    public ClassSessionStatus Status { get; set; } = ClassSessionStatus.Scheduled;

    public Batch Batch { get; set; } = default!;
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = [];
}
