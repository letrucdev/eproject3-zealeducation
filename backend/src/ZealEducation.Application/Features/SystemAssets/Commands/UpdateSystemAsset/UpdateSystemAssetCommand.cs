using System;
using MediatR;

namespace ZealEducation.Application.Features.SystemAssets.Commands.UpdateSystemAsset;

public record UpdateSystemAssetCommand(
    Guid Id,
    string AssetName,
    string AssetType,
    string Location,
    string? Notes) : IRequest;
