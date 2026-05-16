namespace Api.Endpoint.Auth;

public sealed record LoginResult(
	int UserId,
	string AccessToken,
	DateTime ExpiresAtUtc);