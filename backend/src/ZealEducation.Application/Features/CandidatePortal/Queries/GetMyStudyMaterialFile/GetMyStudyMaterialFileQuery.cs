using MediatR;
using ZealEducation.Application.Features.StudyMaterials.Queries.GetStudyMaterialFile;

namespace ZealEducation.Application.Features.CandidatePortal.Queries.GetMyStudyMaterialFile;

public record GetMyStudyMaterialFileQuery(Guid MaterialId) : IRequest<StudyMaterialFileResult>;
