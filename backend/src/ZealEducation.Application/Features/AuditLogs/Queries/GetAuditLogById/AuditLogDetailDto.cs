using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.AuditLogs.Queries.GetAuditLogById;

public class AuditLogDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = default!;
    public string UserFullName { get; set; } = default!;
    public string TableName { get; set; } = default!;
    public string RecordId { get; set; } = default!;
    public AuditAction Action { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? IpAddress { get; set; }
}
