using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Auth.Commands.Login;

namespace ZealEducation.Application.UnitTests.Validators.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Should_pass_when_username_and_password_are_provided()
    {
        var command = new LoginCommand("alice", "Password123!");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_fail_when_username_is_missing(string? username)
    {
        var command = new LoginCommand(username!, "Password123!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Username);
    }

    [Fact]
    public void Should_fail_when_username_exceeds_max_length()
    {
        var command = new LoginCommand(new string('a', 51), "Password123!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Username);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_fail_when_password_is_missing(string? password)
    {
        var command = new LoginCommand("alice", password!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Password);
    }
}
