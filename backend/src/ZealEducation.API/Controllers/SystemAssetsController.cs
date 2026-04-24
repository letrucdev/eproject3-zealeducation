using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Features.SystemAssets.Commands.CreateSystemAsset;
using ZealEducation.Application.Features.SystemAssets.Commands.UpdateSystemAsset;
using ZealEducation.Application.Features.SystemAssets.Commands.UpdateCondition;
using ZealEducation.Application.Features.SystemAssets.Commands.DecommissionAsset;
using ZealEducation.Application.Features.SystemAssets.Queries.GetSystemAssets;
using ZealEducation.Application.Features.SystemAssets.Queries.GetSystemAssetById;
using ZealEducation.Application.Features.SystemAssets.DTOs;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/system-assets")]
[Authorize(Roles = nameof(UserRole.SystemAdmin))]
public class SystemAssetsController : ControllerBase
{
    private readonly ISender _sender;

    public SystemAssetsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SystemAssetDto>>>> GetAll(
        [FromQuery] ConditionStatus? conditionStatus = null,
        [FromQuery] string? assetType = null)
    {
        var result = await _sender.Send(new GetSystemAssetsQuery(conditionStatus, assetType));
        return Ok(ApiResponse<IReadOnlyList<SystemAssetDto>>.Success(result.Data));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SystemAssetDto>>> GetById(Guid id)
    {
        var result = await _sender.Send(new GetSystemAssetByIdQuery(id));
        return Ok(ApiResponse<SystemAssetDto>.Success(result.Data));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] CreateSystemAssetCommand command)
    {
        var result = await _sender.Send(command);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result },
            ApiResponse<Guid>.Success(result, "System asset created successfully"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateSystemAssetRequest body)
    {
        var command = new UpdateSystemAssetCommand(
            id,
            body.AssetName,
            body.AssetType,
            body.SerialNumber,
            body.Location,
            body.PurchaseDate,
            body.Notes);

        await _sender.Send(command);
        return Ok(ApiResponse<object>.Success(null, "System asset updated successfully"));
    }

    [HttpPatch("{id:guid}/condition")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateCondition(Guid id, [FromBody] UpdateConditionRequest body)
    {
        await _sender.Send(new UpdateConditionCommand(id, body.ConditionStatus));
        return Ok(ApiResponse<object>.Success(null, "Condition updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Decommission(Guid id)
    {
        await _sender.Send(new DecommissionAssetCommand(id));
        return Ok(ApiResponse<object>.Success(null, "Asset decommissioned successfully"));
    }

    public record UpdateSystemAssetRequest(
        string AssetName,
        string AssetType,
        string SerialNumber,
        string Location,
        DateTime PurchaseDate,
        string? Notes);

    public record UpdateConditionRequest(ConditionStatus ConditionStatus);
}
