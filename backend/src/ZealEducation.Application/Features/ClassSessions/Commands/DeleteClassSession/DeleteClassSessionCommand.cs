using MediatR;

namespace ZealEducation.Application.Features.ClassSessions.Commands.DeleteClassSession;

public record DeleteClassSessionCommand(Guid SessionId) : IRequest<Unit>;
