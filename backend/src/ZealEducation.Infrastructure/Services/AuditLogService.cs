using System.Text.Json;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Infrastructure.Services;

public class AuditLogService(
    IRepository<AuditLog> auditLogRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IAuditLogService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task LogAsync(
        string tableName,
        string recordId,
        AuditAction action,
        object? oldValue,
        object? newValue,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("Audit log requires an authenticated user.");

        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TableName = tableName,
            RecordId = recordId,
            Action = action,
            OldValue = Serialize(oldValue),
            NewValue = Serialize(newValue),
            ChangedAt = DateTime.UtcNow,
            IpAddress = currentUser.IpAddress
        };

        await auditLogRepository.AddAsync(log, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string? Serialize(object? value)
    {
        if (value is null) return null;
        if (value is string s) return s;
        return JsonSerializer.Serialize(value, SerializerOptions);
    }
}
