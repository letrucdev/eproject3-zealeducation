using MediatR;

namespace ZealEducation.Application.Features.StudyMaterials.Commands.DeleteStudyMaterial;

public record DeleteStudyMaterialCommand(Guid MaterialId) : IRequest;
