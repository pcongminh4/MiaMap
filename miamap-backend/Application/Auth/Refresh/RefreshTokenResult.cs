namespace Application.Auth.Refresh;

public sealed record RefreshTokenResult(
	string AccessToken,
	DateTime AccessTokenExpiresAtUtc,
	string NewRefreshToken,
	DateTime NewRefreshTokenExpiresAtUtc);
