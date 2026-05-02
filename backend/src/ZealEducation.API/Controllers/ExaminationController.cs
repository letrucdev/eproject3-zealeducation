using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.ExamResults.Commands.CreateExamResult;
using ZealEducation.Application.Features.ExamResults.Queries.GetExaminationResults;
using ZealEducation.Application.Features.Examinations.Commands.DeleteExamination;
using ZealEducation.Application.Features.Examinations.Commands.UpdateExamination;
using ZealEducation.Application.Features.Examinations.Queries.GetBatchExaminations;
using ZealEducation.Application.Features.Examinations.Queries.GetExaminationById;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/examinations")]
[Authorize]
public class ExaminationController(ISender sender) : ControllerBase
{
    private const string InchargeOnly = nameof(UserRole.Incharge);
    private const string InchargeOrFaculty = nameof(UserRole.Incharge) + "," + nameof(UserRole.Faculty);

    [HttpGet("{id:guid}")]
    [Authorize(Roles = InchargeOrFaculty)]
    public async Task<ActionResult<ApiResponse<ExaminationDto>>> GetById(Guid id)
    {
        var result = await sender.Send(new GetExaminationByIdQuery(id));
        return Ok(ApiResponse<ExaminationDto>.Success(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateExaminationRequest body)
    {
        await sender.Send(new UpdateExaminationCommand(
            id,
            body.ExamName,
            body.ExamDate,
            body.Location,
            body.MaxScore,
            body.PassScore));

        return Ok(ApiResponse<object>.Success(null, "Examination updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await sender.Send(new DeleteExaminationCommand(id));
        return Ok(ApiResponse<object>.Success(null, "Examination deleted successfully"));
    }

    [HttpGet("{id:guid}/results")]
    [Authorize(Roles = InchargeOrFaculty)]
    public async Task<ActionResult<ApiResponse<PaginatedList<ExamResultDto>>>> GetResults(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await sender.Send(new GetExaminationResultsQuery(id, page, pageSize, sortBy, sortDirection));
        return Ok(ApiResponse<PaginatedList<ExamResultDto>>.Success(result));
    }

    [HttpPost("{id:guid}/results")]
    [Authorize(Roles = InchargeOrFaculty)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateResult(Guid id, [FromBody] CreateExamResultRequest body)
    {
        var resultId = await sender.Send(new CreateExamResultCommand(
            id,
            body.EnrollmentId,
            body.Score,
            body.IsFinalized));

        return Ok(ApiResponse<Guid>.Success(resultId, "Exam result recorded successfully"));
    }

    public record UpdateExaminationRequest(
        string ExamName,
        DateOnly ExamDate,
        string? Location,
        int MaxScore,
        int PassScore);

    public record CreateExamResultRequest(
        Guid EnrollmentId,
        decimal Score,
        bool IsFinalized);
}
