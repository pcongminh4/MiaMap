namespace Application.Users.Login;

public sealed record LoginResponse(
	int UserId,
	string AccessToken,
	DateTime ExpiresAtUtc);
