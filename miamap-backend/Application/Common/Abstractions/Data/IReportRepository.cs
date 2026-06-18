using Domain.Places;

namespace Application.Common.Abstractions.Data;

public interface IReportRepository
{
    Task AddAsync(Report report, CancellationToken cancellationToken = default);
    Task<Report?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Report>> GetActiveReportsInBoundingBoxAsync(
        double minLat,
        double minLng,
        double maxLat,
        double maxLng,
        CancellationToken cancellationToken = default);
}
