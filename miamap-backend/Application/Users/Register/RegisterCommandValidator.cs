using Application.Abstractions.Data;
using Domain.Users;
using FluentValidation;

namespace Application.Users.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
	public RegisterCommandValidator(IUserRepository userRepository)
	{
		RuleFor(command => command.FirstName)
			.NotEmpty()
			.MaximumLength(100);

		RuleFor(command => command.LastName)
			.NotEmpty()
			.MaximumLength(100);

		RuleFor(command => command.Email)
			.NotEmpty()
			.EmailAddress()
			.MaximumLength(255)
			.MustAsync(async (email, cancellationToken) =>
				!await userRepository.ExistsByEmailAsync(email, cancellationToken))
			.WithMessage(command => UserError.EmailAlreadyExists(command.Email).Description);

		RuleFor(command => command.Password)
			.NotEmpty()
			.MinimumLength(8)
			.MaximumLength(128);
	}
}
