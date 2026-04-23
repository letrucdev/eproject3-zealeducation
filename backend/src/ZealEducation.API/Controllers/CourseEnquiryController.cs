using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.CourseEnquiries.Commands.AddEnquiryNote;
using ZealEducation.Application.Features.CourseEnquiries.Commands.ConvertEnquiry;
using ZealEducation.Application.Features.CourseEnquiries.Commands.CreateEnquiry;
using ZealEducation.Application.Features.CourseEnquiries.Commands.UpdateEnquiry;
using ZealEducation.Application.Features.CourseEnquiries.Queries.GetEnquiries;
using ZealEducation.Application.Features.CourseEnquiries.Queries.GetEnquiryById;
using ZealEducation.Application.Features.CourseEnquiries.Queries.GetEnquiryStatistics;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/course-enquiries")]
[Authorize(Roles = nameof(UserRole.Counselor))]
public class CourseEnquiryController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<CourseEnquiryListItemDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] EnquiryStatus? status = null,
        [FromQuery] EnquirySource? source = null,
        [FromQuery] bool? dueFollowUpOnly = null)
    {
        var result = await sender.Send(new GetEnquiriesQuery(page, pageSize, search, status, source, dueFollowUpOnly));
        return Ok(ApiResponse<PaginatedList<CourseEnquiryListItemDto>>.Success(result));
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<EnquiryStatisticsDto>>> GetStatistics()
    {
        var result = await sender.Send(new GetEnquiryStatisticsQuery());
        return Ok(ApiResponse<EnquiryStatisticsDto>.Success(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CourseEnquiryDetailDto>>> GetById(Guid id)
    {
        var result = await sender.Send(new GetEnquiryByIdQuery(id));
        return Ok(ApiResponse<CourseEnquiryDetailDto>.Success(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateEnquiryResponse>>> Create([FromBody] CreateEnquiryCommand command)
    {
        var result = await sender.Send(command);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.EnquiryId },
            ApiResponse<CreateEnquiryResponse>.Success(result, "Enquiry created successfully"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateEnquiryRequest body)
    {
        var command = new UpdateEnquiryCommand(
            id,
            body.FullName,
            body.Phone,
            body.Email,
            body.CourseInterested,
            body.Source,
            body.Status,
            body.NextFollowUpDate);

        await sender.Send(command);
        return Ok(ApiResponse<object>.Success(null, "Enquiry updated successfully"));
    }

    [HttpPost("{id:guid}/notes")]
    public async Task<ActionResult<ApiResponse<AddEnquiryNoteResponse>>> AddNote(Guid id, [FromBody] AddNoteRequest body)
    {
        var result = await sender.Send(new AddEnquiryNoteCommand(id, body.Content));
        return Ok(ApiResponse<AddEnquiryNoteResponse>.Success(result, "Note added successfully"));
    }

    [HttpPost("{id:guid}/convert")]
    public async Task<ActionResult<ApiResponse<ConvertEnquiryResponse>>> Convert(Guid id, [FromBody] ConvertEnquiryRequest body)
    {
        var command = new ConvertEnquiryCommand(
            id,
            body.Email,
            body.Dob,
            body.Gender,
            body.Address,
            body.EmergencyContact);

        var result = await sender.Send(command);
        return Ok(ApiResponse<ConvertEnquiryResponse>.Success(result, "Enquiry converted to candidate successfully"));
    }

    public record UpdateEnquiryRequest(
        string FullName,
        string Phone,
        string? Email,
        string CourseInterested,
        EnquirySource Source,
        EnquiryStatus Status,
        DateOnly? NextFollowUpDate);

    public record AddNoteRequest(string Content);

    public record ConvertEnquiryRequest(
        string Email,
        DateOnly Dob,
        Gender Gender,
        string? Address,
        string? EmergencyContact);
}
