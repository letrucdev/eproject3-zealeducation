using MediatR;

namespace ZealEducation.Application.Features.ExamResults.Commands.OverrideExamResult;

public record OverrideExamResultCommand(
    Guid ResultId,
    decimal Score,
    string OverrideReason) : IRequest<Unit>;
