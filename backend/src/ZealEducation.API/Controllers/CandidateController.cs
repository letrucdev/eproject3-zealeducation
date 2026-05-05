using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.Candidates.Commands.AddEnrollment;
using ZealEducation.Application.Features.Candidates.Commands.ApplyFine;
using ZealEducation.Application.Features.Candidates.Commands.ResetCandidatePassword;
using ZealEducation.Application.Features.Candidates.Commands.UpdateCandidate;
using ZealEducation.Application.Features.Candidates.Queries.GetCandidateDetail;
using ZealEducation.Application.Features.Candidates.Queries.GetCandidates;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/candidates")]
[Authorize(Roles = nameof(UserRole.Incharge))]
public class CandidateController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<CandidateListItemDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] CandidateStatus? status = null,
        [FromQuery] Guid? courseId = null,
        [FromQuery] Guid? batchId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await sender.Send(new GetCandidatesQuery(page, pageSize, search, status, courseId, batchId, sortBy, sortDirection));
        return Ok(ApiResponse<PaginatedList<CandidateListItemDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CandidateDetailDto>>> GetById(Guid id)
    {
        var result = await sender.Send(new GetCandidateDetailQuery(id));
        return Ok(ApiResponse<CandidateDetailDto>.Success(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateCandidateRequest body)
    {
        var command = new UpdateCandidateCommand(
            id,
            body.FullName,
            body.Email,
            body.Phone,
            body.Address,
            body.EmergencyContact,
            body.Notes,
            body.Status);

        await sender.Send(command);
        return Ok(ApiResponse<object>.Success(null, "Candidate updated successfully"));
    }

    [HttpPost("{id:guid}/fines")]
    public async Task<ActionResult<ApiResponse<ApplyFineResponse>>> ApplyFine(Guid id, [FromBody] ApplyFineRequest body)
    {
        var result = await sender.Send(new ApplyFineCommand(id, body.ViolationReason, body.PenaltyAmount));
        return Ok(ApiResponse<ApplyFineResponse>.Success(result, "Fine applied successfully"));
    }

    [HttpPost("{id:guid}/reset-password")]
    public async Task<ActionResult<ApiResponse<ResetCandidatePasswordResponse>>> ResetPassword(Guid id)
    {
        var result = await sender.Send(new ResetCandidatePasswordCommand(id));
        return Ok(ApiResponse<ResetCandidatePasswordResponse>.Success(result, "Candidate password reset successfully"));
    }

    [HttpPost("{id:guid}/enrollments")]
    public async Task<ActionResult<ApiResponse<AddEnrollmentResponse>>> AddEnrollment(
        Guid id,
        [FromBody] AddEnrollmentRequest body)
    {
        var result = await sender.Send(new AddEnrollmentCommand(id, body.CourseId));
        return Ok(ApiResponse<AddEnrollmentResponse>.Success(result, "Enrollment added successfully"));
    }

    public record UpdateCandidateRequest(
        string FullName,
        string Email,
        string Phone,
        string? Address,
        string? EmergencyContact,
        string? Notes,
        CandidateStatus Status);

    public record ApplyFineRequest(string ViolationReason, decimal PenaltyAmount);

    public record AddEnrollmentRequest(Guid CourseId);
}
