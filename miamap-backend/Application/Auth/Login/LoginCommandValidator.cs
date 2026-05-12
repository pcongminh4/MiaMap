using FluentValidation;

namespace Application.Users.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
	public LoginCommandValidator()
	{
		RuleFor(command => command.Email)
			.NotEmpty()
			.EmailAddress()
			.MaximumLength(255);

		RuleFor(command => command.Password)
			.NotEmpty()
			.MinimumLength(8)
			.MaximumLength(128);
	}
}
