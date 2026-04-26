using MediatR;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.ClassSessions.Commands.UpdateClassSession;

public record UpdateClassSessionCommand(
    Guid SessionId,
    DateOnly SessionDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Topic,
    string? Location,
    ClassSessionStatus Status) : IRequest<Unit>;
