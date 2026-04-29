using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Features.Feedback.Commands.SubmitCourseFeedback;
using ZealEducation.Application.Features.Feedback.Commands.SubmitFacultyFeedback;
using ZealEducation.Application.Features.Feedback.Commands.SubmitGeneralFeedback;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/me/feedback")]
[Authorize(Roles = nameof(UserRole.Candidate))]
public class FeedbackController(ISender sender) : ControllerBase
{
    [HttpPost("faculty")]
    public async Task<ActionResult<ApiResponse<Guid>>> SubmitFaculty([FromBody] SubmitFacultyFeedbackCommand command)
    {
        var id = await sender.Send(command);
        return Ok(ApiResponse<Guid>.Success(id, "Faculty feedback submitted."));
    }

    [HttpPost("course")]
    public async Task<ActionResult<ApiResponse<Guid>>> SubmitCourse([FromBody] SubmitCourseFeedbackCommand command)
    {
        var id = await sender.Send(command);
        return Ok(ApiResponse<Guid>.Success(id, "Course feedback submitted."));
    }

    [HttpPost("general")]
    public async Task<ActionResult<ApiResponse<Guid>>> SubmitGeneral([FromBody] SubmitGeneralFeedbackCommand command)
    {
        var id = await sender.Send(command);
        return Ok(ApiResponse<Guid>.Success(id, "General feedback submitted."));
    }
}
