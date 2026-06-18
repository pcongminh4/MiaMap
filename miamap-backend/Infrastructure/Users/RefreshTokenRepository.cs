using Application.Common.Abstractions.Data;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Users;

public sealed class RefreshTokenRepository(ApplicationDbContext dbContext) : IRefreshTokenRepository
{
	public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
	{
		await dbContext.Set<RefreshToken>().AddAsync(refreshToken, cancellationToken);
	}

	public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
	{
		return dbContext.Set<RefreshToken>()
			.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
	}

	public void Remove(RefreshToken refreshToken)
	{
		dbContext.Set<RefreshToken>().Remove(refreshToken);
	}
}
