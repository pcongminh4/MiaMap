using Application.Common.Abstractions.Authentication;
using Application.Common.Abstractions.Data;
using Domain.Users;
using MediatR;

namespace Application.Users.Register;

public sealed class RegisterCommandHandler(
	IUserRepository userRepository,
	IPasswordHasher passwordHasher,
	IUnitOfWork unitOfWork)
	: IRequestHandler<RegisterCommand, int>
{
	public async Task<int> Handle(RegisterCommand request, CancellationToken cancellationToken)
	{
		var normalizedFirstName = request.FirstName.Trim();
		var normalizedLastName = request.LastName.Trim();
		var normalizedEmail = request.Email.Trim().ToLowerInvariant();
		var passwordHash = passwordHasher.Hash(request.Password);

		var user = new User(normalizedFirstName, normalizedLastName, normalizedEmail, passwordHash);

		await userRepository.AddAsync(user, cancellationToken);

		await unitOfWork.SaveChangesAsync(cancellationToken);

		return user.Id;
	}
}

