using System;
using ZealEducation.Domain.Common;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Exceptions;

namespace ZealEducation.Domain.Entities;

public class SystemAsset : BaseAuditableEntity
{
    public string AssetName { get; private set; } = default!;
    public string AssetType { get; private set; } = default!;
    public string SerialNumber { get; private set; } = default!;
    public string Location { get; private set; } = default!;
    public ConditionStatus ConditionStatus { get; private set; } = ConditionStatus.Good;
    public DateOnly PurchaseDate { get; private set; }
    public DateOnly? LastMaintenance { get; private set; }
    public string? Notes { get; private set; }
    public Guid ManagedBy { get; private set; }

    // Constructor required by EF Core
    protected SystemAsset() { }

    public SystemAsset(
        string assetName,
        string assetType,
        string serialNumber,
        string location,
        DateOnly purchaseDate,
        Guid managedBy,
        string? notes = null)
    {
        AssetName = assetName;
        AssetType = assetType;
        SerialNumber = serialNumber;
        Location = location;
        PurchaseDate = purchaseDate;
        ManagedBy = managedBy;
        Notes = notes;
        ConditionStatus = ConditionStatus.Good;
    }

    public void UpdateInfo(
        string assetName,
        string assetType,
        string location,
        string? notes,
        Guid managedBy)
    {
        if (ConditionStatus == ConditionStatus.Decommissioned)
        {
            throw new SystemAssetDecommissionedException("Cannot update a decommissioned asset.");
        }

        AssetName = assetName;
        AssetType = assetType;
        Location = location;
        Notes = notes;
        ManagedBy = managedBy;
    }

    public void UpdateCondition(ConditionStatus newStatus)
    {
        if (ConditionStatus == ConditionStatus.Decommissioned)
        {
            throw new SystemAssetDecommissionedException("Cannot update a decommissioned asset.");
        }

        ConditionStatus = newStatus;

        if (newStatus == ConditionStatus.Maintenance)
        {
            LastMaintenance = DateOnly.FromDateTime(DateTime.UtcNow);
        }
    }

    public void MarkAsMaintained()
    {
        if (ConditionStatus == ConditionStatus.Decommissioned)
        {
            throw new SystemAssetDecommissionedException("Cannot maintain a decommissioned asset.");
        }

        ConditionStatus = ConditionStatus.Good;
        LastMaintenance = DateOnly.FromDateTime(DateTime.UtcNow);
    }

    public void Decommission()
    {
        if (ConditionStatus == ConditionStatus.Decommissioned)
        {
            throw new SystemAssetDecommissionedException("Asset is already decommissioned.");
        }

        ConditionStatus = ConditionStatus.Decommissioned;
    }
}
