using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchAttendance;
using ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchDetail;
using ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchExamResults;
using ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchMaterials;
using ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatchSessions;
using ZealEducation.Application.Features.CandidatePortal.Queries.GetMyBatches;
using ZealEducation.Application.Features.CandidatePortal.Queries.GetMyProfile;
using ZealEducation.Application.Features.CandidatePortal.Queries.GetMyStudyMaterialFile;
using ZealEducation.Application.Features.Candidates.Queries.GetCandidateDetail;
using ZealEducation.Application.Features.ClassSessions.Queries.GetBatchSessions;
using ZealEducation.Application.Features.StudyMaterials.Queries;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/me")]
[Authorize(Roles = nameof(UserRole.Candidate))]
public class MeController(ISender sender) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<CandidateDetailDto>>> GetProfile()
    {
        var result = await sender.Send(new GetMyProfileQuery());
        return Ok(ApiResponse<CandidateDetailDto>.Success(result));
    }

    [HttpGet("batches")]
    public async Task<ActionResult<ApiResponse<PaginatedList<MyBatchListItemDto>>>> GetBatches(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await sender.Send(new GetMyBatchesQuery(page, pageSize, search, sortBy, sortDirection));
        return Ok(ApiResponse<PaginatedList<MyBatchListItemDto>>.Success(result));
    }

    [HttpGet("batches/{id:guid}")]
    public async Task<ActionResult<ApiResponse<MyBatchDetailDto>>> GetBatchDetail(Guid id)
    {
        var result = await sender.Send(new GetMyBatchDetailQuery(id));
        return Ok(ApiResponse<MyBatchDetailDto>.Success(result));
    }

    [HttpGet("batches/{id:guid}/sessions")]
    public async Task<ActionResult<ApiResponse<PaginatedList<ClassSessionDto>>>> GetBatchSessions(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null)
    {
        var result = await sender.Send(new GetMyBatchSessionsQuery(id, page, pageSize, sortBy, sortDirection, fromDate, toDate));
        return Ok(ApiResponse<PaginatedList<ClassSessionDto>>.Success(result));
    }

    [HttpGet("batches/{id:guid}/attendance")]
    public async Task<ActionResult<ApiResponse<MyBatchAttendanceDto>>> GetBatchAttendance(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null)
    {
        var result = await sender.Send(new GetMyBatchAttendanceQuery(id, page, pageSize, sortBy, sortDirection, fromDate, toDate));
        return Ok(ApiResponse<MyBatchAttendanceDto>.Success(result));
    }

    [HttpGet("batches/{id:guid}/exam-results")]
    public async Task<ActionResult<ApiResponse<MyBatchExamResultsDto>>> GetBatchExamResults(Guid id)
    {
        var result = await sender.Send(new GetMyBatchExamResultsQuery(id));
        return Ok(ApiResponse<MyBatchExamResultsDto>.Success(result));
    }

    [HttpGet("batches/{id:guid}/materials")]
    public async Task<ActionResult<ApiResponse<PaginatedList<StudyMaterialListItemDto>>>> GetBatchMaterials(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? search = null)
    {
        var result = await sender.Send(new GetMyBatchMaterialsQuery(id, page, pageSize, search));
        return Ok(ApiResponse<PaginatedList<StudyMaterialListItemDto>>.Success(result));
    }

    [HttpGet("materials/{id:guid}/file")]
    public async Task<IActionResult> GetMaterialFile(Guid id)
    {
        var result = await sender.Send(new GetMyStudyMaterialFileQuery(id));
        return File(result.Content, result.ContentType, result.FileName);
    }
}
