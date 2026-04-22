using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Staffs.Commands.UpdateStaff;

public class UpdateStaffCommandHandler(
    IRepository<Staff> staffRepository,
    IRepository<UserAccount> userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateStaffCommand, Unit>
{
    public async Task<Unit> Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetByIdAsync(request.StaffId, cancellationToken)
            ?? throw new NotFoundException(nameof(Staff), request.StaffId);

        var user = await userRepository.GetByIdAsync(staff.UserAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAccount), staff.UserAccountId);

        var email = request.Email.Trim();
        var phone = request.Phone.Trim();

        var duplicates = await userRepository.FindAsync(
            u => u.Id != user.Id && (u.Email == email || u.Phone == phone),
            cancellationToken);

        if (duplicates.Count > 0)
        {
            var match = duplicates[0];
            if (match.Email == email)
                throw new ConflictException("Email is already in use.");
            throw new ConflictException("Phone number is already in use.");
        }

        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.Phone = phone;
        user.Dob = request.Dob;
        user.Gender = request.Gender;
        user.Role = request.Role;
        user.IsActive = request.IsActive;

        staff.Position = request.Position.Trim();
        staff.Department = request.Department.Trim();
        staff.JoinedDate = request.JoinedDate;

        userRepository.Update(user);
        staffRepository.Update(staff);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
