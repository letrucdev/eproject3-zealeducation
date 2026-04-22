using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.AuditLogs.Queries.GetAuditLogs;

public record GetAuditLogsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    AuditAction? Action = null,
    Guid? UserId = null,
    string? TableName = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IRequest<PaginatedList<AuditLogListItemDto>>;
