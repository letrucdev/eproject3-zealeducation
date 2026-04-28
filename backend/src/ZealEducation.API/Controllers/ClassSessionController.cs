using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Features.ClassSessions.Commands.DeleteClassSession;
using ZealEducation.Application.Features.ClassSessions.Commands.MarkAttendance;
using ZealEducation.Application.Features.ClassSessions.Commands.UpdateClassSession;
using ZealEducation.Application.Features.ClassSessions.Queries.GetSessionAttendance;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/sessions")]
[Authorize(Roles = nameof(UserRole.Incharge))]
public class ClassSessionController(ISender sender) : ControllerBase
{
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateClassSessionRequest body)
    {
        var command = new UpdateClassSessionCommand(
            id,
            body.SessionDate,
            body.StartTime,
            body.EndTime,
            body.Topic,
            body.Location,
            body.Status);

        await sender.Send(command);
        return Ok(ApiResponse<object>.Success(null, "Class session updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await sender.Send(new DeleteClassSessionCommand(id));
        return Ok(ApiResponse<object>.Success(null, "Class session deleted successfully"));
    }

    [HttpGet("{id:guid}/attendance")]
    public async Task<ActionResult<ApiResponse<SessionAttendanceDto>>> GetAttendance(Guid id)
    {
        var result = await sender.Send(new GetSessionAttendanceQuery(id));
        return Ok(ApiResponse<SessionAttendanceDto>.Success(result));
    }

    [HttpPost("{id:guid}/attendance")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAttendance(Guid id, [FromBody] MarkAttendanceRequest body)
    {
        var entries = body.Entries
            .Select(e => new AttendanceEntry(e.EnrollmentId, e.Status, e.PracticalHours, e.Remarks))
            .ToList();

        await sender.Send(new MarkAttendanceCommand(id, entries));
        return Ok(ApiResponse<object>.Success(null, "Attendance saved successfully"));
    }

    public record UpdateClassSessionRequest(
        DateOnly SessionDate,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string? Topic,
        string? Location,
        ClassSessionStatus Status);

    public record MarkAttendanceRequest(List<AttendanceEntryRequest> Entries);

    public record AttendanceEntryRequest(
        Guid EnrollmentId,
        AttendanceStatus Status,
        decimal? PracticalHours,
        string? Remarks);
}
