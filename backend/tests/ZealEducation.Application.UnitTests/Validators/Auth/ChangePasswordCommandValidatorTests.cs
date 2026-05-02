using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Auth.Commands.ChangePassword;

namespace ZealEducation.Application.UnitTests.Validators.Auth;

public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator = new();

    [Fact]
    public void Should_pass_for_a_fully_valid_command()
    {
        var result = _validator.TestValidate(new ChangePasswordCommand("OldPassword1!", "NewPassword2!"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_fail_when_current_password_is_missing(string? currentPassword)
    {
        var result = _validator.TestValidate(new ChangePasswordCommand(currentPassword!, "NewPassword2!"));
        result.ShouldHaveValidationErrorFor(c => c.CurrentPassword);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_fail_when_new_password_is_missing(string? newPassword)
    {
        var result = _validator.TestValidate(new ChangePasswordCommand("OldPassword1!", newPassword!));
        result.ShouldHaveValidationErrorFor(c => c.NewPassword);
    }

    [Fact]
    public void Should_fail_when_new_password_is_shorter_than_8_chars()
    {
        var result = _validator.TestValidate(new ChangePasswordCommand("OldPassword1!", "Short1!"));
        result.ShouldHaveValidationErrorFor(c => c.NewPassword);
    }

    [Fact]
    public void Should_fail_when_new_password_exceeds_100_chars()
    {
        var result = _validator.TestValidate(new ChangePasswordCommand("OldPassword1!", new string('p', 101)));
        result.ShouldHaveValidationErrorFor(c => c.NewPassword);
    }

    [Fact]
    public void Should_fail_when_new_password_equals_current_password()
    {
        var result = _validator.TestValidate(new ChangePasswordCommand("SamePassword1!", "SamePassword1!"));
        result.ShouldHaveValidationErrorFor(c => c.NewPassword);
    }
}
