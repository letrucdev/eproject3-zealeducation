using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ZealEducation.Application.Common.Interfaces;

namespace ZealEducation.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return null;

            var raw = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public string? IpAddress
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            if (context is null) return null;

            var headers = context.Request.Headers;

            var cfConnectingIp = headers["CF-Connecting-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(cfConnectingIp)) return cfConnectingIp.Trim();

            var trueClientIp = headers["True-Client-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(trueClientIp)) return trueClientIp.Trim();

            var forwarded = headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
            {
                var first = forwarded.Split(',')[0].Trim();
                if (!string.IsNullOrWhiteSpace(first)) return first;
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}
