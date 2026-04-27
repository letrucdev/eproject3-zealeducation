using MediatR;

namespace ZealEducation.Application.Features.ExamResults.Commands.CreateExamResult;

public record CreateExamResultCommand(
    Guid ExaminationId,
    Guid EnrollmentId,
    decimal Score,
    string? Grade) : IRequest<Guid>;
