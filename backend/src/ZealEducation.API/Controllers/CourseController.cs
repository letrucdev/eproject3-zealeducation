using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.Courses.Commands.CreateCourse;
using ZealEducation.Application.Features.Courses.Commands.UpdateCourse;
using ZealEducation.Application.Features.Courses.Queries.GetCourseById;
using ZealEducation.Application.Features.Courses.Queries.GetCourses;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/courses")]
[Authorize]
public class CourseController(ISender sender) : ControllerBase
{
    private const string ReadRoles = $"{nameof(UserRole.Incharge)},{nameof(UserRole.Counselor)}";
    private const string WriteRoles = nameof(UserRole.Incharge);

    [HttpGet]
    [Authorize(Roles = ReadRoles)]
    public async Task<ActionResult<ApiResponse<PaginatedList<CourseListItemDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await sender.Send(new GetCoursesQuery(page, pageSize, search, isActive));
        return Ok(ApiResponse<PaginatedList<CourseListItemDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = ReadRoles)]
    public async Task<ActionResult<ApiResponse<CourseDetailDto>>> GetById(Guid id)
    {
        var result = await sender.Send(new GetCourseByIdQuery(id));
        return Ok(ApiResponse<CourseDetailDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Roles = WriteRoles)]
    public async Task<ActionResult<ApiResponse<CreateCourseResponse>>> Create([FromBody] CreateCourseCommand command)
    {
        var result = await sender.Send(command);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.CourseId },
            ApiResponse<CreateCourseResponse>.Success(result, "Course created successfully"));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = WriteRoles)]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateCourseRequest body)
    {
        var command = new UpdateCourseCommand(
            id,
            body.CourseName,
            body.Description,
            body.DurationWeeks,
            body.BaseFee,
            body.IsActive);

        await sender.Send(command);
        return Ok(ApiResponse<object>.Success(null, "Course updated successfully"));
    }

    public record UpdateCourseRequest(
        string CourseName,
        string? Description,
        int DurationWeeks,
        decimal BaseFee,
        bool IsActive);
}
