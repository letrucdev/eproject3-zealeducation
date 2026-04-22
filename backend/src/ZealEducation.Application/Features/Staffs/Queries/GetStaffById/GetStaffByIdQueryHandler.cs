using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Staffs.Queries.GetStaffById;

public class GetStaffByIdQueryHandler(
    IRepository<Staff> staffRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Faculty> facultyRepository) : IRequestHandler<GetStaffByIdQuery, StaffDetailDto>
{
    public async Task<StaffDetailDto> Handle(GetStaffByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await (from s in staffRepository.Query()
                            join u in userRepository.Query() on s.UserAccountId equals u.Id
                            from f in facultyRepository.Query().Where(x => x.StaffId == s.Id).DefaultIfEmpty()
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
                                LastLogin = u.LastLogin,
                                FacultyId = f != null ? (Guid?)f.Id : null,
                                FacultyCode = f != null ? f.FacultyCode : null,
                                Qualification = f != null ? f.Qualification : null,
                                Specialization = f != null ? f.Specialization : null,
                                ExperienceYears = f != null ? (int?)f.ExperienceYears : null
                            }).FirstOrDefaultAsync(cancellationToken);

        return result ?? throw new NotFoundException(nameof(Staff), request.StaffId);
    }
}
