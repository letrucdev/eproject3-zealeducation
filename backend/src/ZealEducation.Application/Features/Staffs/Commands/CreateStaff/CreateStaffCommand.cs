using MediatR;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Staffs.Commands.CreateStaff;

public record CreateStaffCommand(
    string Username,
    string Password,
    string FullName,
    string Email,
    string Phone,
    DateOnly Dob,
    Gender Gender,
    UserRole Role,
    string Position,
    string Department,
    DateOnly? JoinedDate) : IRequest<CreateStaffResponse>;
