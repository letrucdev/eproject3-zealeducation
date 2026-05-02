using MediatR;

namespace ZealEducation.Application.Features.StudyMaterials.Queries.GetStudyMaterialFile;

public record GetStudyMaterialFileQuery(Guid MaterialId) : IRequest<StudyMaterialFileResult>;

public record StudyMaterialFileResult(byte[] Content, string ContentType, string FileName);
