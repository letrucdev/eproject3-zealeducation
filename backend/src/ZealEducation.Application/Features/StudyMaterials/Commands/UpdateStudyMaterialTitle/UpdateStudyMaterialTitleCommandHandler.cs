using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.StudyMaterials.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.StudyMaterials.Commands.UpdateStudyMaterialTitle;

public class UpdateStudyMaterialTitleCommandHandler(
    IRepository<StudyMaterial> materialRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateStudyMaterialTitleCommand>
{
    public async Task Handle(UpdateStudyMaterialTitleCommand request, CancellationToken cancellationToken)
    {
        var material = await materialRepository.GetByIdAsync(request.MaterialId, cancellationToken)
            ?? throw new NotFoundException(nameof(StudyMaterial), request.MaterialId);

        material.Title = request.Title.Trim();
        material.FileName = MaterialFileRules.BuildFileName(material.Title, material.FileName);
        materialRepository.Update(material);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
