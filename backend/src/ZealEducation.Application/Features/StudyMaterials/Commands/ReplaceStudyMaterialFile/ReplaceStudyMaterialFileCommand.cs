using MediatR;

namespace ZealEducation.Application.Features.StudyMaterials.Commands.ReplaceStudyMaterialFile;

public record ReplaceStudyMaterialFileCommand(
    Guid MaterialId,
    Stream FileContent,
    string ContentType,
    string FileName,
    long FileLength) : IRequest;
