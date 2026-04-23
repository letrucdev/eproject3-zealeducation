using System;
using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.SystemAssets.Commands.UpdateSystemAsset;

public record UpdateSystemAssetCommand(
    Guid Id,
    string AssetName,
    string AssetType,
    string SerialNumber,
    string Location,
    string? Notes = null) : IRequest<Result>;
