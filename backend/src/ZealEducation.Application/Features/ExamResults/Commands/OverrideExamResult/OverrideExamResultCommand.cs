using MediatR;

namespace ZealEducation.Application.Features.ExamResults.Commands.OverrideExamResult;

public record OverrideExamResultCommand(
    Guid ResultId,
    decimal Score,
    string? Grade,
    string OverrideReason) : IRequest<Unit>;
