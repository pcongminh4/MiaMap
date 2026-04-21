namespace Domain.Users;

public sealed record UserError(string Code, string Description)
{
	public static readonly UserError InvalidCredentials =
		new("Users.InvalidCredentials", "The provided credentials are invalid.");

	public static UserError EmailAlreadyExists(string email) =>
		new("Users.EmailAlreadyExists", $"A user with email '{email}' already exists.");
}
