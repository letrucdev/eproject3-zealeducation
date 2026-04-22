using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
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
}
