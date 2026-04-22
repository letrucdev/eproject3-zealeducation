using System;
using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.SystemAssets.DTOs;

namespace ZealEducation.Application.Features.SystemAssets.Queries.GetSystemAssetById;

public record GetSystemAssetByIdQuery(Guid Id) : IRequest<Result<SystemAssetDto>>;
