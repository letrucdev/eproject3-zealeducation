using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.AuditLogs.Queries.GetAuditLogById;

public class GetAuditLogByIdQueryHandler(
    IRepository<AuditLog> auditLogRepository,
    IRepository<UserAccount> userRepository) : IRequestHandler<GetAuditLogByIdQuery, AuditLogDetailDto>
{
    public async Task<AuditLogDetailDto> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await (from l in auditLogRepository.Query()
                            join u in userRepository.Query() on l.UserId equals u.Id
                            where l.Id == request.Id
                            select new AuditLogDetailDto
                            {
                                Id = l.Id,
                                UserId = l.UserId,
                                Username = u.Username,
                                UserFullName = u.FullName,
                                TableName = l.TableName,
                                RecordId = l.RecordId,
                                Action = l.Action,
                                OldValue = l.OldValue,
                                NewValue = l.NewValue,
                                ChangedAt = l.ChangedAt,
                                IpAddress = l.IpAddress
                            }).FirstOrDefaultAsync(cancellationToken);

        return result ?? throw new NotFoundException(nameof(AuditLog), request.Id);
    }
}
