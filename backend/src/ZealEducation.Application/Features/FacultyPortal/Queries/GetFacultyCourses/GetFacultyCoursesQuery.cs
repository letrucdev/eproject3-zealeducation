using MediatR;
using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyCourses;

public record GetFacultyCoursesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IRequest<PaginatedList<FacultyCourseOptionDto>>;
