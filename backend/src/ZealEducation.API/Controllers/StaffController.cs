using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.Staffs.Commands.CreateStaff;
using ZealEducation.Application.Features.Staffs.Commands.SetStaffActive;
using ZealEducation.Application.Features.Staffs.Commands.UpdateStaff;
using ZealEducation.Application.Features.Staffs.Queries.GetStaffById;
using ZealEducation.Application.Features.Staffs.Queries.GetStaffs;
using ZealEducation.Application.Features.Staffs.Queries.GetStaffStatistics;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize(Roles = nameof(UserRole.SystemAdmin))]
public class StaffController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<StaffListItemDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] UserRole? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await sender.Send(new GetStaffsQuery(page, pageSize, search, role, isActive, sortBy, sortDirection));
        return Ok(ApiResponse<PaginatedList<StaffListItemDto>>.Success(result));
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<StaffStatisticsDto>>> GetStatistics()
    {
        var result = await sender.Send(new GetStaffStatisticsQuery());
        return Ok(ApiResponse<StaffStatisticsDto>.Success(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StaffDetailDto>>> GetById(Guid id)
    {
        var result = await sender.Send(new GetStaffByIdQuery(id));
        return Ok(ApiResponse<StaffDetailDto>.Success(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateStaffResponse>>> Create([FromBody] CreateStaffCommand command)
    {
        var result = await sender.Send(command);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.StaffId },
            ApiResponse<CreateStaffResponse>.Success(result, "Staff created successfully"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateStaffRequest body)
    {
        var command = new UpdateStaffCommand(
            id,
            body.FullName,
            body.Email,
            body.Phone,
            body.Dob,
            body.Gender,
            body.Role,
            body.Position,
            body.Department,
            body.JoinedDate,
            body.IsActive);

        await sender.Send(command);
        return Ok(ApiResponse<object>.Success(null, "Staff updated successfully"));
    }

    [HttpPatch("{id:guid}/active")]
    public async Task<ActionResult<ApiResponse<object>>> SetActive(Guid id, [FromBody] SetActiveRequest body)
    {
        await sender.Send(new SetStaffActiveCommand(id, body.IsActive));
        var message = body.IsActive ? "Staff activated successfully" : "Staff deactivated successfully";
        return Ok(ApiResponse<object>.Success(null, message));
    }

    public record UpdateStaffRequest(
        string FullName,
        string Email,
        string Phone,
        DateOnly Dob,
        Gender Gender,
        UserRole Role,
        string Position,
        string Department,
        DateOnly JoinedDate,
        bool IsActive);

    public record SetActiveRequest(bool IsActive);
}
