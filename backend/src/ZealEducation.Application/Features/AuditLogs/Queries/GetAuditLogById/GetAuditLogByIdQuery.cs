using MediatR;

namespace ZealEducation.Application.Features.AuditLogs.Queries.GetAuditLogById;

public record GetAuditLogByIdQuery(Guid Id) : IRequest<AuditLogDetailDto>;
