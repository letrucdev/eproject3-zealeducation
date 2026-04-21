using FluentValidation;

namespace ZealEducation.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(50);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
