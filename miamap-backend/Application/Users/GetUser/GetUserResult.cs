namespace Application.Users.GetUser;

public sealed record GetUserResult(
	int Id,
	string FirstName,
	string LastName,
	string Email,
	DateTime CreatedAtUtc);
