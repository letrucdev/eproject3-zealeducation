using MediatR;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;
using FacultyEntity = ZealEducation.Domain.Entities.Faculty;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyCourses;

public class GetFacultyCoursesQueryHandler(
    IRepository<Batch> batchRepository,
    IRepository<FacultyEntity> facultyRepository,
    ICurrentUser currentUser) : IRequestHandler<GetFacultyCoursesQuery, PaginatedList<FacultyCourseOptionDto>>
{
    public async Task<PaginatedList<FacultyCourseOptionDto>> Handle(GetFacultyCoursesQuery request, CancellationToken cancellationToken)
    {
        var facultyId = await facultyRepository.ResolveFacultyIdAsync(currentUser, cancellationToken);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        var distinctCourses = batchRepository.Query()
            .Where(b => b.FacultyId == facultyId)
            .Select(b => new FacultyCourseOptionDto
            {
                CourseId = b.CourseId,
                CourseName = b.Course.CourseName,
            })
            .Distinct();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            distinctCourses = distinctCourses.Where(c => c.CourseName.ToLower().Contains(search));
        }

        var ordered = distinctCourses.OrderBy(c => c.CourseName);

        return await PaginatedList<FacultyCourseOptionDto>.CreateAsync(ordered, page, pageSize);
    }
}
