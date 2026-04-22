using System.Collections.Generic;
using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.SystemAssets.DTOs;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.SystemAssets.Queries.GetSystemAssets;

public record GetSystemAssetsQuery(
    ConditionStatus? ConditionStatus = null,
    string? AssetType = null) : IRequest<Result<IReadOnlyList<SystemAssetDto>>>;
