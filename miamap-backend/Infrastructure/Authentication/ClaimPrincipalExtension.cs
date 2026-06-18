using System.Security.Claims;

namespace Infrastructure.Authentication;

public static class ClaimsPrincipalExtensions
{
	public static int GetUserId(this ClaimsPrincipal principal)
	{
		var claimValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
			?? principal.FindFirstValue("sub");

		if (string.IsNullOrEmpty(claimValue) || !int.TryParse(claimValue, out var userId))
		{
			throw new InvalidOperationException("User ID is missing or invalid in the claims principal.");
		}

		return userId;
	}
}
