namespace Domain.Users;

public sealed class RefreshToken
{
	public int Id { get; private set; }
	public int UserId { get; private set; }
	public string Token { get; private set; } = string.Empty;
	public DateTime ExpiresAtUtc { get; private set; }
	public bool IsRevoked { get; private set; }
	public DateTime CreatedAtUtc { get; private set; }

	public RefreshToken(int userId, string token, DateTime expiresAtUtc)
	{
		UserId = userId;
		Token = token;
		ExpiresAtUtc = expiresAtUtc;
		CreatedAtUtc = DateTime.UtcNow;
		IsRevoked = false;
	}

	private RefreshToken() { }

	public void Revoke()
	{
		IsRevoked = true;
	}

	public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
	public bool IsActive => !IsRevoked && !IsExpired;
}
