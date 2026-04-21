using MediatR;

namespace ZealEducation.Application.Features.Staffs.Queries.GetStaffStatistics;

public record GetStaffStatisticsQuery() : IRequest<StaffStatisticsDto>;
