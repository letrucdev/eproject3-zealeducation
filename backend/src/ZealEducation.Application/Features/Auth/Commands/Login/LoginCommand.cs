using MediatR;

namespace ZealEducation.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<LoginResponse>;
