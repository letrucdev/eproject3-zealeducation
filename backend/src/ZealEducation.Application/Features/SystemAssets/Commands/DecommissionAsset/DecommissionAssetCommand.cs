using System;
using MediatR;

namespace ZealEducation.Application.Features.SystemAssets.Commands.DecommissionAsset;

public record DecommissionAssetCommand(Guid Id) : IRequest;
