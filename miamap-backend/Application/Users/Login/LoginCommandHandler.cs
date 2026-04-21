using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Users;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Users.Login;

public sealed class LoginCommandHandler(
	IUserRepository userRepository,
	IPasswordHasher passwordHasher,
	IJwtService jwtService)
	: IRequestHandler<LoginCommand, LoginResponse>
{
	public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
	{
		var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

		if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
		{
			throw new ValidationException(new[]
			{
				new ValidationFailure(nameof(LoginCommand.Password), UserError.InvalidCredentials.Description)
			});
		}

		var token = jwtService.GenerateToken(user);

		return new LoginResponse(user.Id, token.AccessToken, token.ExpiresAtUtc);
	}
}

