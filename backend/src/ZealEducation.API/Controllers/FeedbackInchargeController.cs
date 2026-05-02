using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.Feedback.Commands.SetFeedbackProcessed;
using ZealEducation.Application.Features.Feedback.Queries.GetFeedbacks;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/incharge/feedback")]
[Authorize(Roles = nameof(UserRole.Incharge))]
public class FeedbackInchargeController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<FeedbackListItemDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] FeedbackType? type = null,
        [FromQuery] Guid? batchId = null,
        [FromQuery] int? rating = null,
        [FromQuery] bool? isProcessed = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await sender.Send(new GetFeedbacksQuery(
            page, pageSize, search, type, batchId, rating, isProcessed, sortBy, sortDirection));
        return Ok(ApiResponse<PaginatedList<FeedbackListItemDto>>.Success(result));
    }

    [HttpPatch("{id:guid}/processed")]
    public async Task<ActionResult<ApiResponse<object>>> SetProcessed(
        Guid id, [FromBody] SetProcessedRequest body)
    {
        await sender.Send(new SetFeedbackProcessedCommand(id, body.IsProcessed));
        return Ok(ApiResponse<object>.Success(
            null,
            body.IsProcessed ? "Feedback marked as processed." : "Feedback marked as unprocessed."));
    }

    public record SetProcessedRequest(bool IsProcessed);
}
