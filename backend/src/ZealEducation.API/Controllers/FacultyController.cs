using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.Batches.Queries.GetBatchById;
using ZealEducation.Application.Features.Batches.Queries.GetBatchEnrollments;
using ZealEducation.Application.Features.Batches.Queries.GetBatches;
using ZealEducation.Application.Features.ClassSessions.Commands.MarkAttendance;
using ZealEducation.Application.Features.ClassSessions.Queries.GetBatchSessions;
using ZealEducation.Application.Features.ClassSessions.Queries.GetSessionAttendance;
using ZealEducation.Application.Features.Faculties.Commands.CreateFaculty;
using ZealEducation.Application.Features.Faculties.Commands.UpdateFaculty;
using ZealEducation.Application.Features.Faculties.Queries.GetFaculties;
using ZealEducation.Application.Features.FacultyPortal.Commands.MarkFacultyAttendance;
using ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyBatchById;
using ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyBatchEnrollments;
using ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyBatches;
using ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyBatchSessions;
using ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyCourses;
using ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyExaminationCandidates;
using ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyExaminations;
using ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultySchedule;
using ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultySessionAttendance;
using ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyUpcomingSessions;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/faculty")]
[Authorize]
public class FacultyController(ISender sender) : ControllerBase
{
    private const string ReadRoles = $"{nameof(UserRole.SystemAdmin)},{nameof(UserRole.Incharge)}";
    private const string WriteRoles = nameof(UserRole.SystemAdmin);
    private const string FacultyOnly = nameof(UserRole.Faculty);

    [HttpGet]
    [Authorize(Roles = ReadRoles)]
    public async Task<ActionResult<ApiResponse<PaginatedList<FacultyListItemDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await sender.Send(new GetFacultiesQuery(page, pageSize, search));
        return Ok(ApiResponse<PaginatedList<FacultyListItemDto>>.Success(result));
    }

    [HttpPost]
    [Authorize(Roles = WriteRoles)]
    public async Task<ActionResult<ApiResponse<CreateFacultyResponse>>> Create([FromBody] CreateFacultyCommand command)
    {
        var result = await sender.Send(command);
        return CreatedAtAction(
            nameof(Create),
            new { id = result.FacultyId },
            ApiResponse<CreateFacultyResponse>.Success(result, "Faculty created successfully"));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = WriteRoles)]
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

    // ===== Faculty self-service endpoints (current logged-in faculty) =====

    [HttpGet("me/batches")]
    [Authorize(Roles = FacultyOnly)]
    public async Task<ActionResult<ApiResponse<PaginatedList<BatchListItemDto>>>> GetMyBatches(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] BatchStatus? status = null,
        [FromQuery] Guid? courseId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await sender.Send(new GetFacultyBatchesQuery(page, pageSize, search, status, courseId, sortBy, sortDirection));
        return Ok(ApiResponse<PaginatedList<BatchListItemDto>>.Success(result));
    }

    [HttpGet("me/courses")]
    [Authorize(Roles = FacultyOnly)]
    public async Task<ActionResult<ApiResponse<PaginatedList<FacultyCourseOptionDto>>>> GetMyCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var result = await sender.Send(new GetFacultyCoursesQuery(page, pageSize, search));
        return Ok(ApiResponse<PaginatedList<FacultyCourseOptionDto>>.Success(result));
    }

    [HttpGet("me/batches/{id:guid}")]
    [Authorize(Roles = FacultyOnly)]
    public async Task<ActionResult<ApiResponse<BatchDetailDto>>> GetMyBatchById(Guid id)
    {
        var result = await sender.Send(new GetFacultyBatchByIdQuery(id));
        return Ok(ApiResponse<BatchDetailDto>.Success(result));
    }

    [HttpGet("me/batches/{id:guid}/sessions")]
    [Authorize(Roles = FacultyOnly)]
    public async Task<ActionResult<ApiResponse<PaginatedList<ClassSessionDto>>>> GetMyBatchSessions(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await sender.Send(new GetFacultyBatchSessionsQuery(id, page, pageSize, sortBy, sortDirection));
        return Ok(ApiResponse<PaginatedList<ClassSessionDto>>.Success(result));
    }

    [HttpGet("me/batches/{id:guid}/enrollments")]
    [Authorize(Roles = FacultyOnly)]
    public async Task<ActionResult<ApiResponse<PaginatedList<BatchEnrollmentItemDto>>>> GetMyBatchEnrollments(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await sender.Send(new GetFacultyBatchEnrollmentsQuery(id, page, pageSize, search, sortBy, sortDirection));
        return Ok(ApiResponse<PaginatedList<BatchEnrollmentItemDto>>.Success(result));
    }

    [HttpGet("me/schedule")]
    [Authorize(Roles = FacultyOnly)]
    public async Task<ActionResult<ApiResponse<PaginatedList<FacultyScheduleItemDto>>>> GetMySchedule(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? batchId = null,
        [FromQuery] Guid? courseId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await sender.Send(new GetFacultyScheduleQuery(from, to, page, pageSize, batchId, courseId, sortBy, sortDirection));
        return Ok(ApiResponse<PaginatedList<FacultyScheduleItemDto>>.Success(result));
    }

    [HttpGet("me/upcoming-sessions")]
    [Authorize(Roles = FacultyOnly)]
    public async Task<ActionResult<ApiResponse<List<FacultyScheduleItemDto>>>> GetMyUpcomingSessions()
    {
        var result = await sender.Send(new GetFacultyUpcomingSessionsQuery());
        return Ok(ApiResponse<List<FacultyScheduleItemDto>>.Success(result));
    }

    [HttpGet("me/sessions/{id:guid}/attendance")]
    [Authorize(Roles = FacultyOnly)]
    public async Task<ActionResult<ApiResponse<SessionAttendanceDto>>> GetMySessionAttendance(Guid id)
    {
        var result = await sender.Send(new GetFacultySessionAttendanceQuery(id));
        return Ok(ApiResponse<SessionAttendanceDto>.Success(result));
    }

    [HttpPost("me/sessions/{id:guid}/attendance")]
    [Authorize(Roles = FacultyOnly)]
    public async Task<ActionResult<ApiResponse<object>>> MarkMyAttendance(Guid id, [FromBody] FacultyMarkAttendanceRequest body)
    {
        var entries = body.Entries
            .Select(e => new AttendanceEntry(e.EnrollmentId, e.Status, e.PracticalHours, e.Remarks))
            .ToList();

        await sender.Send(new MarkFacultyAttendanceCommand(id, entries));
        return Ok(ApiResponse<object>.Success(null, "Attendance saved successfully"));
    }

    [HttpGet("me/examinations")]
    [Authorize(Roles = FacultyOnly)]
    public async Task<ActionResult<ApiResponse<PaginatedList<FacultyExaminationSummaryDto>>>> GetMyExaminations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? batchId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await sender.Send(new GetFacultyExaminationsQuery(page, pageSize, search, batchId, sortBy, sortDirection));
        return Ok(ApiResponse<PaginatedList<FacultyExaminationSummaryDto>>.Success(result));
    }

    [HttpGet("me/examinations/{id:guid}/candidates")]
    [Authorize(Roles = FacultyOnly)]
    public async Task<ActionResult<ApiResponse<List<FacultyExaminationCandidateDto>>>> GetMyExaminationCandidates(Guid id)
    {
        var result = await sender.Send(new GetFacultyExaminationCandidatesQuery(id));
        return Ok(ApiResponse<List<FacultyExaminationCandidateDto>>.Success(result));
    }

    public record FacultyMarkAttendanceRequest(List<FacultyAttendanceEntryRequest> Entries);

    public record FacultyAttendanceEntryRequest(
        Guid EnrollmentId,
        AttendanceStatus Status,
        decimal? PracticalHours,
        string? Remarks);
}
