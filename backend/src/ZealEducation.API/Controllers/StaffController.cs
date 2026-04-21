using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Features.Staffs.Commands.CreateStaff;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize(Roles = nameof(UserRole.SystemAdmin))]
public class StaffController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateStaffResponse>>> Create([FromBody] CreateStaffCommand command)
    {
        var result = await sender.Send(command);
        return CreatedAtAction(
            nameof(Create),
            new { id = result.StaffId },
            ApiResponse<CreateStaffResponse>.Success(result, "Staff created successfully"));
    }
}
