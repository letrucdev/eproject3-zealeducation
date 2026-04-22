using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler(
    IRepository<AuditLog> auditLogRepository,
    IRepository<UserAccount> userRepository) : IRequestHandler<GetAuditLogsQuery, PaginatedList<AuditLogListItemDto>>
{
    private const int PreviewLength = 200;

    public async Task<PaginatedList<AuditLogListItemDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var logs = auditLogRepository.Query();
        var users = userRepository.Query();

        var query = from l in logs
                    join u in users on l.UserId equals u.Id
                    select new { Log = l, User = u };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Log.TableName.ToLower().Contains(search) ||
                x.Log.RecordId.ToLower().Contains(search) ||
                x.User.Username.ToLower().Contains(search) ||
                x.User.FullName.ToLower().Contains(search));
        }

        if (request.Action.HasValue)
        {
            var action = request.Action.Value;
            query = query.Where(x => x.Log.Action == action);
        }

        if (request.UserId.HasValue)
        {
            var userId = request.UserId.Value;
            query = query.Where(x => x.Log.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(request.TableName))
        {
            var tableName = request.TableName.Trim();
            query = query.Where(x => x.Log.TableName == tableName);
        }

        if (request.FromDate.HasValue)
        {
            var from = request.FromDate.Value;
            query = query.Where(x => x.Log.ChangedAt >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = request.ToDate.Value;
            query = query.Where(x => x.Log.ChangedAt <= to);
        }

        var projected = query
            .OrderByDescending(x => x.Log.ChangedAt)
            .Select(x => new AuditLogListItemDto
            {
                Id = x.Log.Id,
                UserId = x.Log.UserId,
                Username = x.User.Username,
                UserFullName = x.User.FullName,
                TableName = x.Log.TableName,
                RecordId = x.Log.RecordId,
                Action = x.Log.Action,
                OldValuePreview = x.Log.OldValue == null
                    ? null
                    : x.Log.OldValue.Length > PreviewLength ? x.Log.OldValue.Substring(0, PreviewLength) : x.Log.OldValue,
                NewValuePreview = x.Log.NewValue == null
                    ? null
                    : x.Log.NewValue.Length > PreviewLength ? x.Log.NewValue.Substring(0, PreviewLength) : x.Log.NewValue,
                ChangedAt = x.Log.ChangedAt,
                IpAddress = x.Log.IpAddress
            });

        return await PaginatedList<AuditLogListItemDto>.CreateAsync(projected, page, pageSize);
    }
}
