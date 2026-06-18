using Application.Common.Abstractions.Data;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Users.GetUser;

public sealed class GetUserQueryHandler(IUserRepository userRepository)
	: IRequestHandler<GetUserQuery, GetUserResult>
{
	public async Task<GetUserResult> Handle(GetUserQuery request, CancellationToken cancellationToken)
	{
		var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

		if (user is null)
		{
			throw new ValidationException(new[]
			{
				new ValidationFailure("UserId", "User not found.")
			});
		}

		return new GetUserResult(
			user.Id,
			user.FirstName,
			user.LastName,
			user.Email,
			user.CreatedAtUtc);
	}
}
