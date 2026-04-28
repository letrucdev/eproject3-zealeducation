using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.StudyMaterials.Commands.DeleteStudyMaterial;

public class DeleteStudyMaterialCommandHandler(
    IRepository<StudyMaterial> materialRepository,
    IFileStorageService fileStorage,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteStudyMaterialCommand>
{
    public async Task Handle(DeleteStudyMaterialCommand request, CancellationToken cancellationToken)
    {
        var filePath = await materialRepository.Query()
            .Where(m => m.Id == request.MaterialId)
            .Select(m => m.FilePath)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(StudyMaterial), request.MaterialId);

        var siblings = await materialRepository.Query()
            .Where(m => m.FilePath == filePath)
            .ToListAsync(cancellationToken);

        foreach (var sibling in siblings)
        {
            materialRepository.Delete(sibling);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        try { await fileStorage.DeleteAsync(filePath, CancellationToken.None); } catch { /* best-effort */ }
    }
}
