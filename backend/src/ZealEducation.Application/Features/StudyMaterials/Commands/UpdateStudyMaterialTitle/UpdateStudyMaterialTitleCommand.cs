using MediatR;

namespace ZealEducation.Application.Features.StudyMaterials.Commands.UpdateStudyMaterialTitle;

public record UpdateStudyMaterialTitleCommand(Guid MaterialId, string Title) : IRequest;
