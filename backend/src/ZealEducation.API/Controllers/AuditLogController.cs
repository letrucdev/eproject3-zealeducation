using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.AuditLogs.Queries.GetAuditLogById;
using ZealEducation.Application.Features.AuditLogs.Queries.GetAuditLogs;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = nameof(UserRole.SystemAdmin))]
public class AuditLogController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<AuditLogListItemDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] AuditAction? action = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? tableName = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var result = await sender.Send(
            new GetAuditLogsQuery(page, pageSize, search, action, userId, tableName, fromDate, toDate));
        return Ok(ApiResponse<PaginatedList<AuditLogListItemDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AuditLogDetailDto>>> GetById(Guid id)
    {
        var result = await sender.Send(new GetAuditLogByIdQuery(id));
        return Ok(ApiResponse<AuditLogDetailDto>.Success(result));
    }
}
