using MediatR;

namespace ZealEducation.Application.Features.FacultyPortal.Queries.GetFacultyExaminationCandidates;

public record GetFacultyExaminationCandidatesQuery(Guid ExaminationId) : IRequest<List<FacultyExaminationCandidateDto>>;
