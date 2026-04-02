using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.Application.Features.Courses.Commands.CreateCourse;
using ZealEducation.Application.Features.Courses.Queries.GetCourses;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ISender _sender;

    public CoursesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<List<CourseDto>>> GetAll()
    {
        var result = await _sender.Send(new GetCoursesQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateCourseCommand command)
    {
        var id = await _sender.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id }, id);
    }
}
