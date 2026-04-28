using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.StudyMaterials.Commands.ToggleStudyMaterialActive;

public class ToggleStudyMaterialActiveCommandHandler(
    IRepository<StudyMaterial> materialRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ToggleStudyMaterialActiveCommand>
{
    public async Task Handle(ToggleStudyMaterialActiveCommand request, CancellationToken cancellationToken)
    {
        var material = await materialRepository.GetByIdAsync(request.MaterialId, cancellationToken)
            ?? throw new NotFoundException(nameof(StudyMaterial), request.MaterialId);

        material.IsActive = request.IsActive;
        materialRepository.Update(material);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
