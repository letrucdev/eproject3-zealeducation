using MediatR;

namespace ZealEducation.Application.Features.ClassSessions.Commands.CreateClassSession;

public record CreateClassSessionCommand(
    Guid BatchId,
    DateOnly SessionDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Topic,
    string? Location) : IRequest<Guid>;
