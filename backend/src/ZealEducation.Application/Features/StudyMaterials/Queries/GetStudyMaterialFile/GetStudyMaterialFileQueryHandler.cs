using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.StudyMaterials.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.StudyMaterials.Queries.GetStudyMaterialFile;

public class GetStudyMaterialFileQueryHandler(
    IRepository<StudyMaterial> materialRepository,
    IFileStorageService fileStorage) : IRequestHandler<GetStudyMaterialFileQuery, StudyMaterialFileResult>
{
    public async Task<StudyMaterialFileResult> Handle(GetStudyMaterialFileQuery request, CancellationToken cancellationToken)
    {
        var material = await materialRepository.GetByIdAsync(request.MaterialId, cancellationToken)
            ?? throw new NotFoundException(nameof(StudyMaterial), request.MaterialId);

        var bytes = await fileStorage.DownloadAsync(material.FilePath, cancellationToken);
        var contentType = MaterialFileRules.ResolveContentType(material.FileName);
        return new StudyMaterialFileResult(bytes, contentType, material.FileName);
    }
}
