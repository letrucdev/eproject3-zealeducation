using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Staffs.Queries.GetStaffs;

public class GetStaffsQueryHandler(
    IRepository<Staff> staffRepository,
    IRepository<UserAccount> userRepository) : IRequestHandler<GetStaffsQuery, PaginatedList<StaffListItemDto>>
{
    private static readonly UserRole[] StaffRoles =
    [
        UserRole.Incharge,
        UserRole.Counselor,
        UserRole.Faculty,
        UserRole.AccountsStaff
    ];

    public async Task<PaginatedList<StaffListItemDto>> Handle(GetStaffsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var staffs = staffRepository.Query();
        var users = userRepository.Query().Where(u => StaffRoles.Contains(u.Role));

        var query = from s in staffs
                    join u in users on s.UserAccountId equals u.Id
                    select new { Staff = s, User = u };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.User.Username.ToLower().Contains(search) ||
                x.User.FullName.ToLower().Contains(search) ||
                x.User.Email.ToLower().Contains(search) ||
                x.User.Phone.Contains(search));
        }

        if (request.Role.HasValue)
        {
            var role = request.Role.Value;
            query = query.Where(x => x.User.Role == role);
        }

        if (request.IsActive.HasValue)
        {
            var isActive = request.IsActive.Value;
            query = query.Where(x => x.User.IsActive == isActive);
        }

        var projected = query
            .OrderBy(x => x.User.FullName)
            .Select(x => new StaffListItemDto
            {
                StaffId = x.Staff.Id,
                UserAccountId = x.User.Id,
                Username = x.User.Username,
                FullName = x.User.FullName,
                Email = x.User.Email,
                Phone = x.User.Phone,
                Dob = x.User.Dob,
                Gender = x.User.Gender,
                Role = x.User.Role,
                IsActive = x.User.IsActive,
                Position = x.Staff.Position,
                Department = x.Staff.Department,
                JoinedDate = x.Staff.JoinedDate,
                LastLogin = x.User.LastLogin
            });

        return await PaginatedList<StaffListItemDto>.CreateAsync(projected, page, pageSize);
    }
}
