using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    IRepository<UserAccount> userRepository,
    IRepository<Staff> staffRepository,
    IRepository<Faculty> facultyRepository,
    IRepository<Candidate> candidateRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var matchedUsers = await userRepository.FindAsync(u => u.Username == request.Username, cancellationToken);
        var user = matchedUsers[0];

        if (user is null || !user.IsActive)
            throw new UnauthorizedException("Invalid username or password.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginCount += 1;
            userRepository.Update(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid username or password.");
        }

        user.FailedLoginCount = 0;
        user.LastLogin = DateTime.UtcNow;
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var tokenResult = jwtTokenService.GenerateToken(user);

        var response = new LoginResponse
        {
            Token = tokenResult.Token,
            ExpiresAt = tokenResult.ExpiresAt,
            User = MapUser(user)
        };

        await PopulateRoleInfoAsync(response, user, cancellationToken);

        return response;
    }

    private async Task PopulateRoleInfoAsync(LoginResponse response, UserAccount user, CancellationToken cancellationToken)
    {
        switch (user.Role)
        {
            case UserRole.Candidate:
                {
                    var candidates = await candidateRepository.FindAsync(c => c.UserAccountId == user.Id, cancellationToken);
                    if (candidates.Count > 0)
                        response.User.Candidate = MapCandidate(candidates[0]);
                    break;
                }
            case UserRole.Faculty:
                {
                    var staffList = await staffRepository.FindAsync(s => s.UserAccountId == user.Id, cancellationToken);
                    if (staffList.Count == 0) break;

                    var staff = staffList[0];
                    response.User.Staff = MapStaff(staff);

                    var facultyList = await facultyRepository.FindAsync(f => f.StaffId == staff.Id, cancellationToken);
                    if (facultyList.Count > 0)
                        response.User.Faculty = MapFaculty(facultyList[0]);
                    break;
                }
            case UserRole.SystemAdmin:
            case UserRole.Incharge:
            case UserRole.Counselor:
            case UserRole.AccountsStaff:
                {
                    var staffList = await staffRepository.FindAsync(s => s.UserAccountId == user.Id, cancellationToken);
                    if (staffList.Count > 0)
                        response.User.Staff = MapStaff(staffList[0]);
                    break;
                }
        }
    }

    private static UserAccountDto MapUser(UserAccount user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        FullName = user.FullName,
        Email = user.Email,
        Phone = user.Phone,
        Dob = user.Dob,
        Gender = user.Gender,
        Role = user.Role,
        IsActive = user.IsActive,
        LastLogin = user.LastLogin
    };

    private static StaffInfoDto MapStaff(Staff staff) => new()
    {
        Id = staff.Id,
        Position = staff.Position,
        Department = staff.Department,
        JoinedDate = staff.JoinedDate,
        IsActive = staff.IsActive
    };

    private static FacultyInfoDto MapFaculty(Faculty faculty) => new()
    {
        Id = faculty.Id,
        StaffId = faculty.StaffId,
        FacultyCode = faculty.FacultyCode,
        Qualification = faculty.Qualification,
        Specialization = faculty.Specialization,
        ExperienceYears = faculty.ExperienceYears
    };

    private static CandidateInfoDto MapCandidate(Candidate candidate) => new()
    {
        Id = candidate.Id,
        CandidateCode = candidate.CandidateCode,
        Address = candidate.Address,
        EmergencyContact = candidate.EmergencyContact,
        Status = candidate.Status,
        RegisteredAt = candidate.RegisteredAt
    };
}
