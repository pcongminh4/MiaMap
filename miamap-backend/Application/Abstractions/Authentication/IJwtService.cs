using Domain.Users;

namespace Application.Abstractions.Authentication;

public interface IJwtService
{
	JwtToken GenerateToken(User user);
}

public sealed record JwtToken(string AccessToken, DateTime ExpiresAtUtc);

