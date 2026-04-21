using MediatR;

namespace Application.Users.Login;

public sealed record LoginCommand(
	string Email,
	string Password) : IRequest<LoginResponse>;
