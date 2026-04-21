using Domain.Users;

namespace Application.Abstractions.Data;

public interface IUserRepository
{
	Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

	Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

	Task AddAsync(User user, CancellationToken cancellationToken = default);
}
