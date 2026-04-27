using MediatR;

namespace ZealEducation.Application.Features.Examinations.Commands.UpdateExamination;

public record UpdateExaminationCommand(
    Guid ExaminationId,
    string ExamName,
    DateOnly ExamDate,
    string? Location,
    int MaxScore,
    int PassScore) : IRequest<Unit>;
