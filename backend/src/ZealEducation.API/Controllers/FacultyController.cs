using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Features.Faculties.Commands.CreateFaculty;
using ZealEducation.Application.Features.Faculties.Commands.UpdateFaculty;
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

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateFacultyRequest body)
    {
        var command = new UpdateFacultyCommand(
            id,
            body.FullName,
            body.Email,
            body.Phone,
            body.Dob,
            body.Gender,
            body.Position,
            body.Department,
            body.JoinedDate,
            body.IsActive,
            body.FacultyCode,
            body.Qualification,
            body.Specialization,
            body.ExperienceYears);

        await sender.Send(command);
        return Ok(ApiResponse<object>.Success(null, "Faculty updated successfully"));
    }

    public record UpdateFacultyRequest(
        string FullName,
        string Email,
        string Phone,
        DateOnly Dob,
        Gender Gender,
        string Position,
        string Department,
        DateOnly JoinedDate,
        bool IsActive,
        string FacultyCode,
        string Qualification,
        string Specialization,
        int ExperienceYears);
}
