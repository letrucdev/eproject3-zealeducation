using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Staffs.Commands.CreateStaff;

public class CreateStaffCommandHandler(
    IRepository<UserAccount> userRepository,
    IRepository<Staff> staffRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher) : IRequestHandler<CreateStaffCommand, CreateStaffResponse>
{
    public async Task<CreateStaffResponse> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();
        var phone = request.Phone.Trim();

        var existing = await userRepository.FindAsync(
            u => u.Username == username || u.Email == email || u.Phone == phone,
            cancellationToken);

        if (existing.Count > 0)
        {
            var match = existing[0];
            if (match.Username == username)
                throw new ConflictException("Username is already in use.");
            if (match.Email == email)
                throw new ConflictException("Email is already in use.");
            throw new ConflictException("Phone number is already in use.");
        }

        var userAccount = new UserAccount
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = phone,
            Dob = request.Dob,
            Gender = request.Gender,
            Role = request.Role,
            IsActive = true,
            FailedLoginCount = 0
        };

        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            UserAccountId = userAccount.Id,
            Position = request.Position.Trim(),
            Department = request.Department.Trim(),
            JoinedDate = request.JoinedDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true
        };

        await userRepository.AddAsync(userAccount, cancellationToken);
        await staffRepository.AddAsync(staff, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateStaffResponse
        {
            UserAccountId = userAccount.Id,
            StaffId = staff.Id,
            Username = userAccount.Username,
            FullName = userAccount.FullName,
            Email = userAccount.Email,
            Phone = userAccount.Phone,
            Role = userAccount.Role,
            Position = staff.Position,
            Department = staff.Department,
            JoinedDate = staff.JoinedDate
        };
    }
}
