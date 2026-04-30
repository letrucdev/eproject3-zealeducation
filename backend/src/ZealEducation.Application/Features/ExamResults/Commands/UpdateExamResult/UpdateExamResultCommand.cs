using MediatR;

namespace ZealEducation.Application.Features.ExamResults.Commands.UpdateExamResult;

public record UpdateExamResultCommand(
    Guid ResultId,
    decimal Score,
    bool IsFinalized) : IRequest<Unit>;
