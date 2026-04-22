using MediatR;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Faculties.Commands.UpdateFaculty;

public record UpdateFacultyCommand(
    Guid StaffId,
    string FullName,
    string Email,
    string Phone,
    DateOnly Dob,
    Gender Gender,
    string Position,
    string Department,
    DateOnly JoinedDate,
    bool IsActive,
    string FacultyCode,
    string Qualification,
    string Specialization,
    int ExperienceYears) : IRequest<Unit>;
