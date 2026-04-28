using MediatR;

namespace ZealEducation.Application.Features.StudyMaterials.Commands.ToggleStudyMaterialActive;

public record ToggleStudyMaterialActiveCommand(Guid MaterialId, bool IsActive) : IRequest;
