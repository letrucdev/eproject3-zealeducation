using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Staffs.Queries.GetStaffStatistics;

public class GetStaffStatisticsQueryHandler(
    IRepository<Staff> staffRepository,
    IRepository<UserAccount> userRepository) : IRequestHandler<GetStaffStatisticsQuery, StaffStatisticsDto>
{
    private static readonly UserRole[] StaffRoles =
    [
        UserRole.Incharge,
        UserRole.Counselor,
        UserRole.AccountsStaff,
        UserRole.Faculty
    ];

    public async Task<StaffStatisticsDto> Handle(GetStaffStatisticsQuery request, CancellationToken cancellationToken)
    {
        var staffs = staffRepository.Query();
        var users = userRepository.Query().Where(u => StaffRoles.Contains(u.Role));

        var query = from s in staffs
                    join u in users on s.UserAccountId equals u.Id
                    select new { u.Role, u.IsActive };

        var rows = await query.ToListAsync(cancellationToken);

        return new StaffStatisticsDto
        {
            Total = rows.Count,
            Active = rows.Count(x => x.IsActive),
            Inactive = rows.Count(x => !x.IsActive),
            Incharge = rows.Count(x => x.Role == UserRole.Incharge),
            Counselor = rows.Count(x => x.Role == UserRole.Counselor),
            AccountsStaff = rows.Count(x => x.Role == UserRole.AccountsStaff)
        };
    }
}
