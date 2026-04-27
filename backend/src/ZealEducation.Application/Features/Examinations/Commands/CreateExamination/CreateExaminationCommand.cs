using MediatR;

namespace ZealEducation.Application.Features.Examinations.Commands.CreateExamination;

public record CreateExaminationCommand(
    Guid BatchId,
    string ExamName,
    DateOnly ExamDate,
    string? Location,
    int MaxScore,
    int PassScore) : IRequest<Guid>;
