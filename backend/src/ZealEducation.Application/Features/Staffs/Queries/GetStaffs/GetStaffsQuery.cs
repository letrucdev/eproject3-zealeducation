using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Staffs.Queries.GetStaffs;

public record GetStaffsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    UserRole? Role = null,
    bool? IsActive = null,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<StaffListItemDto>>;
