using System;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.SystemAssets.DTOs;

public class SystemAssetDto
{
    public Guid Id { get; init; }
    public string AssetName { get; init; } = default!;
    public string AssetType { get; init; } = default!;
    public string SerialNumber { get; init; } = default!;
    public string Location { get; init; } = default!;
    public ConditionStatus ConditionStatus { get; init; }
    public DateOnly PurchaseDate { get; init; }
    public DateOnly? LastMaintenance { get; init; }
    public string? Notes { get; init; }
    public Guid ManagedBy { get; init; }
    public string? ManagedByName { get; init; }
}
