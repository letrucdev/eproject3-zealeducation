using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Features.Faculties.Commands.CreateFaculty;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/faculty")]
[Authorize(Roles = nameof(UserRole.SystemAdmin))]
public class FacultyController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateFacultyResponse>>> Create([FromBody] CreateFacultyCommand command)
    {
        var result = await sender.Send(command);
        return CreatedAtAction(
            nameof(Create),
            new { id = result.FacultyId },
            ApiResponse<CreateFacultyResponse>.Success(result, "Faculty created successfully"));
    }
}
