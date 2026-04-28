using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Features.Auth.Commands.ChangePassword;
using ZealEducation.Application.Features.Auth.Commands.Login;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginCommand command)
    {
        var result = await sender.Send(command);
        return Ok(ApiResponse<LoginResponse>.Success(result, "Login successful"));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        await sender.Send(command);
        return Ok(ApiResponse<object>.Success(null, "Password changed successfully"));
    }
}
