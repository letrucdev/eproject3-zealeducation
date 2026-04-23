using System;
using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.SystemAssets.Commands.CreateSystemAsset;

public record CreateSystemAssetCommand(
    string AssetName,
    string AssetType,
    string SerialNumber,
    string Location,
    DateTime PurchaseDate,
    string? Notes = null) : IRequest<Result<Guid>>;
