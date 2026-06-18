using System.Security.Cryptography;
using Api.Endpoint.Auth;
using Application.Common.Abstractions.Authentication;
using Application.Common.Abstractions.Data;
using Domain.Users;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Users.Login;

public sealed class LoginCommandHandler(
	IUserRepository userRepository,
	IRefreshTokenRepository refreshTokenRepository,
	IPasswordHasher passwordHasher,
	IJwtService jwtService,
	IUnitOfWork unitOfWork)
	: IRequestHandler<LoginCommand, LoginResult>
{
	public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
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

		var refreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
		var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
		var refreshToken = new RefreshToken(user.Id, refreshTokenString, refreshTokenExpiresAt);

		await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return new LoginResult(
			user.Id,
			token.AccessToken,
			token.ExpiresAtUtc,
			refreshTokenString,
			refreshTokenExpiresAt);
	}
}

