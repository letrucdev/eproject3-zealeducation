using System;
using MediatR;

namespace ZealEducation.Application.Features.SystemAssets.Commands.CreateSystemAsset;

public record CreateSystemAssetCommand(
    string AssetName,
    string AssetType,
    string SerialNumber,
    string Location,
    DateTime PurchaseDate,
    string? Notes = null) : IRequest<Guid>;
    
// No ManagedBy — automatically retrieved from JWT token
