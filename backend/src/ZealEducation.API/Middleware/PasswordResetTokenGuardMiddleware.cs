using System.Text.Json;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Common.Constants;

namespace ZealEducation.API.Middleware;

public class PasswordResetTokenGuardMiddleware(RequestDelegate next)
{
    private const string AllowedPath = "/api/auth/change-password";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var purpose = user.FindFirst(AuthClaimTypes.TokenPurpose)?.Value;
            if (purpose == TokenPurposes.PasswordReset)
            {
                var path = context.Request.Path.Value ?? string.Empty;
                if (!path.Equals(AllowedPath, StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    var payload = JsonSerializer.Serialize(
                        ApiResponse<object>.Error("This token can only be used to change password."),
                        JsonOptions);
                    await context.Response.WriteAsync(payload);
                    return;
                }
            }
        }

        await next(context);
    }
}
