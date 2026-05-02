using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Features.ExamResults.Commands.DeleteExamResult;
using ZealEducation.Application.Features.ExamResults.Commands.OverrideExamResult;
using ZealEducation.Application.Features.ExamResults.Commands.UpdateExamResult;
using ZealEducation.Application.Features.ExamResults.Queries.GetExamResultById;
using ZealEducation.Application.Features.ExamResults.Queries.GetExaminationResults;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/exam-results")]
[Authorize]
public class ExamResultController(ISender sender) : ControllerBase
{
    private const string InchargeOnly = nameof(UserRole.Incharge);
    private const string InchargeOrFaculty = nameof(UserRole.Incharge) + "," + nameof(UserRole.Faculty);

    [HttpGet("{id:guid}")]
    [Authorize(Roles = InchargeOrFaculty)]
    public async Task<ActionResult<ApiResponse<ExamResultDto>>> GetById(Guid id)
    {
        var result = await sender.Send(new GetExamResultByIdQuery(id));
        return Ok(ApiResponse<ExamResultDto>.Success(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = InchargeOrFaculty)]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateExamResultRequest body)
    {
        await sender.Send(new UpdateExamResultCommand(id, body.Score, body.IsFinalized));
        return Ok(ApiResponse<object>.Success(null, "Exam result updated successfully"));
    }

    [HttpPut("{id:guid}/override")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<object>>> Override(Guid id, [FromBody] OverrideExamResultRequest body)
    {
        await sender.Send(new OverrideExamResultCommand(id, body.Score, body.OverrideReason));
        return Ok(ApiResponse<object>.Success(null, "Exam result overridden successfully"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await sender.Send(new DeleteExamResultCommand(id));
        return Ok(ApiResponse<object>.Success(null, "Exam result deleted successfully"));
    }

    public record UpdateExamResultRequest(decimal Score, bool IsFinalized);
    public record OverrideExamResultRequest(decimal Score, string OverrideReason);
}
