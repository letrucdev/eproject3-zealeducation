using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.Batches.Commands.AssignCandidatesToBatch;
using ZealEducation.Application.Features.Batches.Commands.AssignFaculty;
using ZealEducation.Application.Features.Batches.Commands.CreateBatch;
using ZealEducation.Application.Features.Batches.Commands.DeleteBatch;
using ZealEducation.Application.Features.Batches.Commands.UpdateBatch;
using ZealEducation.Application.Features.Batches.Queries.GetAssignableCandidates;
using ZealEducation.Application.Features.Batches.Queries.GetBatchById;
using ZealEducation.Application.Features.Batches.Queries.GetBatchEnrollments;
using ZealEducation.Application.Features.Batches.Queries.GetBatches;
using ZealEducation.Application.Features.Batches.Queries.GetBatchStatistics;
using ZealEducation.Application.Features.ClassSessions.Commands.CreateBulkSessions;
using ZealEducation.Application.Features.ClassSessions.Commands.CreateClassSession;
using ZealEducation.Application.Features.ClassSessions.Queries.GetBatchSessions;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/batches")]
[Authorize]
public class BatchController(ISender sender) : ControllerBase
{
    private const string InchargeOnly = nameof(UserRole.Incharge);

    [HttpGet]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<PaginatedList<BatchListItemDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? courseId = null,
        [FromQuery] Guid? facultyId = null,
        [FromQuery] BatchStatus? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await sender.Send(new GetBatchesQuery(page, pageSize, search, courseId, facultyId, status, sortBy, sortDirection));
        return Ok(ApiResponse<PaginatedList<BatchListItemDto>>.Success(result));
    }

    [HttpGet("statistics")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<BatchStatisticsDto>>> GetStatistics()
    {
        var result = await sender.Send(new GetBatchStatisticsQuery());
        return Ok(ApiResponse<BatchStatisticsDto>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<BatchDetailDto>>> GetById(Guid id)
    {
        var result = await sender.Send(new GetBatchByIdQuery(id));
        return Ok(ApiResponse<BatchDetailDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<CreateBatchResponse>>> Create([FromBody] CreateBatchCommand command)
    {
        var result = await sender.Send(command);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.BatchId },
            ApiResponse<CreateBatchResponse>.Success(result, "Batch created successfully"));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateBatchRequest body)
    {
        var command = new UpdateBatchCommand(
            id,
            body.BatchCode,
            body.CourseId,
            body.StartDate,
            body.EndDate,
            body.Location,
            body.MaxCapacity,
            body.Status);

        await sender.Send(command);
        return Ok(ApiResponse<object>.Success(null, "Batch updated successfully"));
    }

    [HttpPatch("{id:guid}/faculty")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<object>>> AssignFaculty(Guid id, [FromBody] AssignFacultyRequest body)
    {
        await sender.Send(new AssignFacultyCommand(id, body.FacultyId));
        return Ok(ApiResponse<object>.Success(null, "Faculty assignment updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await sender.Send(new DeleteBatchCommand(id));
        return Ok(ApiResponse<object>.Success(null, "Batch deleted successfully"));
    }

    [HttpGet("{id:guid}/enrollments")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<PaginatedList<BatchEnrollmentItemDto>>>> GetEnrollments(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await sender.Send(new GetBatchEnrollmentsQuery(id, page, pageSize, search, sortBy, sortDirection));
        return Ok(ApiResponse<PaginatedList<BatchEnrollmentItemDto>>.Success(result));
    }

    [HttpGet("{id:guid}/assignable-candidates")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<PaginatedList<AssignableCandidateDto>>>> GetAssignableCandidates(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await sender.Send(new GetAssignableCandidatesQuery(id, page, pageSize, search));
        return Ok(ApiResponse<PaginatedList<AssignableCandidateDto>>.Success(result));
    }

    [HttpPost("{id:guid}/candidates")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<object>>> AssignCandidates(Guid id, [FromBody] AssignCandidatesRequest body)
    {
        await sender.Send(new AssignCandidatesToBatchCommand(id, body.EnrollmentIds));
        return Ok(ApiResponse<object>.Success(null, "Candidates assigned to batch successfully"));
    }

    [HttpGet("{id:guid}/sessions")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<PaginatedList<ClassSessionDto>>>> GetSessions(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await sender.Send(new GetBatchSessionsQuery(id, page, pageSize, sortBy, sortDirection));
        return Ok(ApiResponse<PaginatedList<ClassSessionDto>>.Success(result));
    }

    [HttpPost("{id:guid}/sessions")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateSession(Guid id, [FromBody] CreateClassSessionRequest body)
    {
        var sessionId = await sender.Send(new CreateClassSessionCommand(
            id,
            body.SessionDate,
            body.StartTime,
            body.EndTime,
            body.Topic,
            body.Location));

        return Ok(ApiResponse<Guid>.Success(sessionId, "Class session created successfully"));
    }

    [HttpPost("{id:guid}/sessions/bulk")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<CreateBulkSessionsResponse>>> CreateBulkSessions(
        Guid id, [FromBody] CreateBulkSessionsRequest body)
    {
        var result = await sender.Send(new CreateBulkSessionsCommand(
            id,
            body.DaysOfWeek,
            body.StartTime,
            body.EndTime,
            body.Topic,
            body.Location));

        return Ok(ApiResponse<CreateBulkSessionsResponse>.Success(result, "Sessions created successfully"));
    }

    public record UpdateBatchRequest(
        string BatchCode,
        Guid CourseId,
        DateOnly StartDate,
        DateOnly EndDate,
        string? Location,
        int MaxCapacity,
        BatchStatus Status);

    public record AssignFacultyRequest(Guid? FacultyId);

    public record AssignCandidatesRequest(List<Guid> EnrollmentIds);

    public record CreateClassSessionRequest(
        DateOnly SessionDate,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string? Topic,
        string? Location);

    public record CreateBulkSessionsRequest(
        List<DayOfWeek> DaysOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string? Topic,
        string? Location);
}
