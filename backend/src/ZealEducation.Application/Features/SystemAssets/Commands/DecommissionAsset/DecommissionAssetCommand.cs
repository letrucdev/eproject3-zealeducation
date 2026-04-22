using System;
using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.SystemAssets.Commands.DecommissionAsset;

public record DecommissionAssetCommand(Guid Id) : IRequest<Result>;
