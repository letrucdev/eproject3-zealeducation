using MediatR;

namespace ZealEducation.Application.Features.Staffs.Commands.SetStaffActive;

public record SetStaffActiveCommand(Guid StaffId, bool IsActive) : IRequest<Unit>;
