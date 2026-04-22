using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Infrastructure.Data.Interceptors;

public class AuditLogInterceptor(ICurrentUser currentUser) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static readonly HashSet<string> SensitivePropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash",
        "Password",
        "Secret",
        "Token",
        "RefreshToken"
    };

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        CaptureAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void CaptureAuditLogs(DbContext? context)
    {
        if (context is null) return;

        var userId = currentUser.UserId;
        if (userId is null) return;

        var ipAddress = currentUser.IpAddress;
        var now = DateTime.UtcNow;

        var entries = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity is not AuditLog)
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var action = entry.State switch
            {
                EntityState.Added => AuditAction.INSERT,
                EntityState.Modified => AuditAction.UPDATE,
                EntityState.Deleted => AuditAction.DELETE,
                _ => throw new InvalidOperationException($"Unexpected state {entry.State}")
            };

            context.Set<AuditLog>().Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                TableName = entry.Metadata.GetTableName() ?? entry.Metadata.ClrType.Name,
                RecordId = entry.Entity.Id.ToString(),
                Action = action,
                OldValue = action == AuditAction.INSERT ? null : SerializeValues(entry.OriginalValues),
                NewValue = action == AuditAction.DELETE ? null : SerializeValues(entry.CurrentValues),
                ChangedAt = now,
                IpAddress = ipAddress
            });
        }
    }

    private static string SerializeValues(PropertyValues values)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var property in values.Properties)
        {
            if (SensitivePropertyNames.Contains(property.Name))
            {
                dict[property.Name] = "***";
                continue;
            }
            dict[property.Name] = values[property];
        }
        return JsonSerializer.Serialize(dict, SerializerOptions);
    }
}
