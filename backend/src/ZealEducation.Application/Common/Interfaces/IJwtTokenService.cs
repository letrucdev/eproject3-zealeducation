using ZealEducation.Domain.Entities;

namespace ZealEducation.Application.Common.Interfaces;

public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(UserAccount user);
    JwtTokenResult GeneratePasswordResetToken(UserAccount user);
}

public record JwtTokenResult(string Token, DateTime ExpiresAt);
