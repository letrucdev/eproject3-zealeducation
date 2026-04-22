using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
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
[Route("api/[controller]")]
public class SystemAssetsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] CreateSystemAssetCommand command)
    {
        var result = await sender.Send(command);
        
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<Guid>.Error(string.Join(", ", result.Errors)));
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data },
            ApiResponse<Guid>.Success(result.Data, "System asset created successfully"));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SystemAssetDto>>>> GetAll([FromQuery] ConditionStatus? conditionStatus, [FromQuery] string? assetType)
    {
        var result = await sender.Send(new GetSystemAssetsQuery(conditionStatus, assetType));
        return Ok(ApiResponse<IReadOnlyList<SystemAssetDto>>.Success(result.Data));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SystemAssetDto>>> GetById(Guid id)
    {
        var result = await sender.Send(new GetSystemAssetByIdQuery(id));
        return Ok(ApiResponse<SystemAssetDto>.Success(result.Data));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateSystemAssetCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(ApiResponse<object>.Error("Id mismatch"));
        }

        var result = await sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.Error(string.Join(", ", result.Errors)));
        }

        return Ok(ApiResponse<object>.Success(null, "System asset updated successfully"));
    }

    [HttpPatch("{id:guid}/condition")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateCondition(Guid id, [FromBody] UpdateConditionCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(ApiResponse<object>.Error("Id mismatch"));
        }

        var result = await sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.Error(string.Join(", ", result.Errors)));
        }

        return Ok(ApiResponse<object>.Success(null, "System asset condition updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var result = await sender.Send(new DecommissionAssetCommand(id));

        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.Error(string.Join(", ", result.Errors)));
        }

        return Ok(ApiResponse<object>.Success(null, "System asset decommissioned successfully"));
    }
}
