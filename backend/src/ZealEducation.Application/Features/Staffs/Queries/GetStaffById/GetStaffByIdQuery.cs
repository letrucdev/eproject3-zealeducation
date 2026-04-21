using MediatR;

namespace ZealEducation.Application.Features.Staffs.Queries.GetStaffById;

public record GetStaffByIdQuery(Guid StaffId) : IRequest<StaffDetailDto>;
