using ZealEducation.Domain.Enums;

namespace ZealEducation.Domain.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(
        string tableName,
        string recordId,
        AuditAction action,
        object? oldValue,
        object? newValue,
        CancellationToken cancellationToken = default);
}
