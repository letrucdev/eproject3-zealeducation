using MediatR;

namespace ZealEducation.Application.Features.Examinations.Commands.DeleteExamination;

public record DeleteExaminationCommand(Guid ExaminationId) : IRequest<Unit>;
