using Application.Common.Abstractions.Data;
using Domain.Places;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Infrastructure.Places;

public sealed class ReportRepository(ApplicationDbContext dbContext) : IReportRepository
{
    public async Task AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<Report>().AddAsync(report, cancellationToken);
    }

    public Task<Report?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Report>()
            .Include(r => r.Votes)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Report>> GetActiveReportsInBoundingBoxAsync(
        double minLat,
        double minLng,
        double maxLat,
        double maxLng,
        CancellationToken cancellationToken = default)
    {
        var boundingBox = new GeometryFactory(new PrecisionModel(), 4326)
            .ToGeometry(new Envelope(minLng, maxLng, minLat, maxLat));

        return await dbContext.Set<Report>()
            .Where(r => r.IsActive && r.ExpiresAtUtc > DateTime.UtcNow)
            .Where(r => r.Location.Intersects(boundingBox))
            .ToListAsync(cancellationToken);
    }
}
