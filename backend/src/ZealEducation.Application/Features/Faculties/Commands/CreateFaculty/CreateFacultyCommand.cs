using MediatR;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Faculties.Commands.CreateFaculty;

public record CreateFacultyCommand(
    string Username,
    string Password,
    string FullName,
    string Email,
    string Phone,
    DateOnly Dob,
    Gender Gender,
    string Position,
    string Department,
    DateOnly? JoinedDate,
    string FacultyCode,
    string Qualification,
    string Specialization,
    int ExperienceYears) : IRequest<CreateFacultyResponse>;
