using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Faculties.Commands.UpdateFaculty;

public class UpdateFacultyCommandHandler(
    IRepository<Staff> staffRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Faculty> facultyRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateFacultyCommand, Unit>
{
    public async Task<Unit> Handle(UpdateFacultyCommand request, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetByIdAsync(request.StaffId, cancellationToken)
            ?? throw new NotFoundException(nameof(Staff), request.StaffId);

        var user = await userRepository.GetByIdAsync(staff.UserAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAccount), staff.UserAccountId);

        if (user.Role != UserRole.Faculty)
            throw new ConflictException("Staff is not a faculty member.");

        var facultyMatches = await facultyRepository.FindAsync(
            f => f.StaffId == staff.Id,
            cancellationToken);
        var faculty = facultyMatches.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Faculty), staff.Id);

        var email = request.Email.Trim();
        var phone = request.Phone.Trim();
        var facultyCode = request.FacultyCode.Trim();

        var duplicateUsers = await userRepository.FindAsync(
            u => u.Id != user.Id && (u.Email == email || u.Phone == phone),
            cancellationToken);

        if (duplicateUsers.Count > 0)
        {
            var match = duplicateUsers[0];
            if (match.Email == email)
                throw new ConflictException("Email is already in use.");
            throw new ConflictException("Phone number is already in use.");
        }

        var duplicateFaculties = await facultyRepository.FindAsync(
            f => f.Id != faculty.Id && f.FacultyCode == facultyCode,
            cancellationToken);

        if (duplicateFaculties.Count > 0)
            throw new ConflictException("Faculty code is already in use.");

        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.Phone = phone;
        user.Dob = request.Dob;
        user.Gender = request.Gender;
        user.IsActive = request.IsActive;

        staff.Position = request.Position.Trim();
        staff.Department = request.Department.Trim();
        staff.JoinedDate = request.JoinedDate;

        faculty.FacultyCode = facultyCode;
        faculty.Qualification = request.Qualification.Trim();
        faculty.Specialization = request.Specialization.Trim();
        faculty.ExperienceYears = request.ExperienceYears;

        userRepository.Update(user);
        staffRepository.Update(staff);
        facultyRepository.Update(faculty);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
