using Domain.Users;

namespace Application.Common.Abstractions.Data;

public interface IRefreshTokenRepository
{
	Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
	Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
	void Remove(RefreshToken refreshToken);
}
