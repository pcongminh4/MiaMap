using FluentValidation;

namespace Application.Auth.Refresh;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
	public RefreshTokenCommandValidator()
	{
		RuleFor(command => command.RefreshToken)
			.NotEmpty()
			.WithMessage("Refresh token is required.");
	}
}
