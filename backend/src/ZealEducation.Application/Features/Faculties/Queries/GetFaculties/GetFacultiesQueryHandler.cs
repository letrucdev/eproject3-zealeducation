using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Faculties.Queries.GetFaculties;

public class GetFacultiesQueryHandler(
    IRepository<Faculty> facultyRepository) : IRequestHandler<GetFacultiesQuery, PaginatedList<FacultyListItemDto>>
{
    public async Task<PaginatedList<FacultyListItemDto>> Handle(GetFacultiesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var query = facultyRepository.Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(f =>
                f.FacultyCode.ToLower().Contains(search) ||
                f.Specialization.ToLower().Contains(search) ||
                f.Staff.UserAccount.FullName.ToLower().Contains(search) ||
                f.Staff.UserAccount.Email.ToLower().Contains(search));
        }

        var projected = query
            .OrderBy(f => f.Staff.UserAccount.FullName)
            .Select(f => new FacultyListItemDto
            {
                FacultyId = f.Id,
                FacultyCode = f.FacultyCode,
                FullName = f.Staff.UserAccount.FullName,
                Email = f.Staff.UserAccount.Email,
                Phone = f.Staff.UserAccount.Phone,
                Qualification = f.Qualification,
                Specialization = f.Specialization,
                ExperienceYears = f.ExperienceYears,
                IsActive = f.Staff.IsActive && f.Staff.UserAccount.IsActive,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            });

        return await PaginatedList<FacultyListItemDto>.CreateAsync(projected, page, pageSize);
    }
}
