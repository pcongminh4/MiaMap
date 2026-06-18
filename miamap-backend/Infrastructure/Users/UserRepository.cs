using Application.Common.Abstractions.Data;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Users;

public sealed class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
	public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
	{
		var normalizedEmail = email.Trim().ToLowerInvariant();

		return dbContext.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken);
	}

	public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
	{
		var normalizedEmail = email.Trim().ToLowerInvariant();

		return dbContext.Users.FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
	}

	public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
	{
		return dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
	}

	public Task AddAsync(User user, CancellationToken cancellationToken = default)
	{
		return dbContext.Users.AddAsync(user, cancellationToken).AsTask();
	}
}
