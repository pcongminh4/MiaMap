using System.Security.Cryptography;
using Application.Common.Abstractions.Authentication;
using Application.Common.Abstractions.Data;
using Domain.Users;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Auth.Refresh;

public sealed class RefreshTokenCommandHandler(
	IRefreshTokenRepository refreshTokenRepository,
	IUserRepository userRepository,
	IJwtService jwtService,
	IUnitOfWork unitOfWork)
	: IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
	public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
	{
		var refreshToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

		if (refreshToken is null || !refreshToken.IsActive)
		{
			throw new ValidationException(new[]
			{
				new ValidationFailure("RefreshToken", "Refresh token is invalid or expired.")
			});
		}

		var user = await userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
		if (user is null)
		{
			throw new ValidationException(new[]
			{
				new ValidationFailure("RefreshToken", "User not found.")
			});
		}

		// Tạo Access Token mới
		var jwtToken = jwtService.GenerateToken(user);

		// Tạo Refresh Token mới (Xoay vòng token)
		var newRefreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
		var newRefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
		var newRefreshToken = new RefreshToken(user.Id, newRefreshTokenString, newRefreshTokenExpiresAt);

		// Thu hồi token cũ
		refreshToken.Revoke();

		await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return new RefreshTokenResult(
			jwtToken.AccessToken,
			jwtToken.ExpiresAtUtc,
			newRefreshTokenString,
			newRefreshTokenExpiresAt);
	}
}
