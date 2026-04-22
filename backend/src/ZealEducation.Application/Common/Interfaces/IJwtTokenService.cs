using ZealEducation.Domain.Entities;

namespace ZealEducation.Application.Common.Interfaces;

public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(UserAccount user);
}

public record JwtTokenResult(string Token, DateTime ExpiresAt);
