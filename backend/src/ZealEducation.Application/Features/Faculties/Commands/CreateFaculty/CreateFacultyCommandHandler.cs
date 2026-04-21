using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Faculties.Commands.CreateFaculty;

public class CreateFacultyCommandHandler(
    IRepository<UserAccount> userRepository,
    IRepository<Staff> staffRepository,
    IRepository<Faculty> facultyRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher) : IRequestHandler<CreateFacultyCommand, CreateFacultyResponse>
{
    public async Task<CreateFacultyResponse> Handle(CreateFacultyCommand request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();
        var phone = request.Phone.Trim();
        var facultyCode = request.FacultyCode.Trim();

        var existingUsers = await userRepository.FindAsync(
            u => u.Username == username || u.Email == email || u.Phone == phone,
            cancellationToken);

        if (existingUsers.Count > 0)
        {
            var match = existingUsers[0];
            if (match.Username == username)
                throw new ConflictException("Username is already in use.");
            if (match.Email == email)
                throw new ConflictException("Email is already in use.");
            throw new ConflictException("Phone number is already in use.");
        }

        var existingFaculty = await facultyRepository.FindAsync(
            f => f.FacultyCode == facultyCode,
            cancellationToken);

        if (existingFaculty.Count > 0)
            throw new ConflictException("Faculty code is already in use.");

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
            Role = UserRole.Faculty,
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

        var faculty = new Faculty
        {
            Id = Guid.NewGuid(),
            StaffId = staff.Id,
            FacultyCode = facultyCode,
            Qualification = request.Qualification.Trim(),
            Specialization = request.Specialization.Trim(),
            ExperienceYears = request.ExperienceYears
        };

        await userRepository.AddAsync(userAccount, cancellationToken);
        await staffRepository.AddAsync(staff, cancellationToken);
        await facultyRepository.AddAsync(faculty, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateFacultyResponse
        {
            UserAccountId = userAccount.Id,
            StaffId = staff.Id,
            FacultyId = faculty.Id,
            Username = userAccount.Username,
            FullName = userAccount.FullName,
            Email = userAccount.Email,
            Phone = userAccount.Phone,
            Role = userAccount.Role,
            Position = staff.Position,
            Department = staff.Department,
            JoinedDate = staff.JoinedDate,
            FacultyCode = faculty.FacultyCode,
            Qualification = faculty.Qualification,
            Specialization = faculty.Specialization,
            ExperienceYears = faculty.ExperienceYears
        };
    }
}
