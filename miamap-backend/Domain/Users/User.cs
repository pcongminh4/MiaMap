namespace Domain.Users;

public sealed class User
{
	public User(string firstName, string lastName, string email, string passwordHash)
	{
		FirstName = firstName;
		LastName = lastName;
		Email = email;
		PasswordHash = passwordHash;
		CreatedAtUtc = DateTime.UtcNow;
	}

	private User()
	{
	}

	public int Id { get; private set; }

	public string FirstName { get; private set; } = string.Empty;

	public string LastName { get; private set; } = string.Empty;

	public string Email { get; private set; } = string.Empty;

	public string PasswordHash { get; private set; } = string.Empty;

	public DateTime CreatedAtUtc { get; private set; }

	public DateTime? UpdatedAtUtc { get; private set; }

	public void UpdateName(string firstName, string lastName)
	{
		FirstName = firstName.Trim();
		LastName = lastName.Trim();
		UpdatedAtUtc = DateTime.UtcNow;
	}

	public void ChangePassword(string passwordHash)
	{
		PasswordHash = passwordHash;
		UpdatedAtUtc = DateTime.UtcNow;
	}
}
