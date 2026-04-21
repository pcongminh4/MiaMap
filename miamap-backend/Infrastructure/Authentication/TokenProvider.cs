using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Abstractions.Authentication;
using Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

public sealed class TokenProvider(IConfiguration configuration) : IJwtService
{
	public JwtToken GenerateToken(User user)
	{
		var issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
		var audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing.");
		var secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is missing.");

		if (!int.TryParse(configuration["Jwt:ExpiryMinutes"], out var expiryMinutes))
		{
			throw new InvalidOperationException("Jwt:ExpiryMinutes is invalid.");
		}

		var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes);
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new(JwtRegisteredClaimNames.Email, user.Email),
			new(ClaimTypes.NameIdentifier, user.Id.ToString()),
			new(ClaimTypes.Email, user.Email)
		};

		var token = new JwtSecurityToken(
			issuer: issuer,
			audience: audience,
			claims: claims,
			expires: expiresAtUtc,
			signingCredentials: credentials);

		var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

		return new JwtToken(accessToken, expiresAtUtc);
	}
}

