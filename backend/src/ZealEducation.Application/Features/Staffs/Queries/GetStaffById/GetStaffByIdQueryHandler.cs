using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Staffs.Queries.GetStaffById;

public class GetStaffByIdQueryHandler(
    IRepository<Staff> staffRepository,
    IRepository<UserAccount> userRepository) : IRequestHandler<GetStaffByIdQuery, StaffDetailDto>
{
    public async Task<StaffDetailDto> Handle(GetStaffByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await (from s in staffRepository.Query()
                            join u in userRepository.Query() on s.UserAccountId equals u.Id
                            where s.Id == request.StaffId
                            select new StaffDetailDto
                            {
                                StaffId = s.Id,
                                UserAccountId = u.Id,
                                Username = u.Username,
                                FullName = u.FullName,
                                Email = u.Email,
                                Phone = u.Phone,
                                Dob = u.Dob,
                                Gender = u.Gender,
                                Role = u.Role,
                                IsActive = u.IsActive,
                                Position = s.Position,
                                Department = s.Department,
                                JoinedDate = s.JoinedDate,
                                LastLogin = u.LastLogin
                            }).FirstOrDefaultAsync(cancellationToken);

        return result ?? throw new NotFoundException(nameof(Staff), request.StaffId);
    }
}
