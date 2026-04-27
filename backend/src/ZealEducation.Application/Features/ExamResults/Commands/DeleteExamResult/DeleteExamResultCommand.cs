using MediatR;

namespace ZealEducation.Application.Features.ExamResults.Commands.DeleteExamResult;

public record DeleteExamResultCommand(Guid ResultId) : IRequest<Unit>;
