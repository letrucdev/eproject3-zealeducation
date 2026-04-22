using MediatR;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Staffs.Commands.UpdateStaff;

public record UpdateStaffCommand(
    Guid StaffId,
    string FullName,
    string Email,
    string Phone,
    DateOnly Dob,
    Gender Gender,
    UserRole Role,
    string Position,
    string Department,
    DateOnly JoinedDate,
    bool IsActive) : IRequest<Unit>;
