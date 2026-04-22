using System;
using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.SystemAssets.Commands.UpdateCondition;

public record UpdateConditionCommand(
    Guid Id,
    ConditionStatus NewStatus) : IRequest<Result>;
