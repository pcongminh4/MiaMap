using Application.Abstractions.Data;
using Domain.Places;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Places;

public sealed class PlaceRepository(ApplicationDbContext dbContext) : IPlaceRepository
{
	public Task AddAsync(Place place, CancellationToken cancellationToken = default)
	{
		return dbContext.Places.AddAsync(place, cancellationToken).AsTask();
	}

	public async Task<PlaceUpsertResult> UpsertExternalAsync(
		ExternalPlaceUpsertRequest request,
		CancellationToken cancellationToken = default)
	{
		var normalizedSource = request.Source.Trim().ToLowerInvariant();
		var normalizedExternalType = request.ExternalType.Trim().ToLowerInvariant();

		var existingPlace = await dbContext.Places
			.SingleOrDefaultAsync(
				place => place.Source == normalizedSource && place.ExternalId == request.ExternalId,
				cancellationToken);

		if (existingPlace is null)
		{
			var createdPlace = Place.CreateFromExternal(
				normalizedSource,
				request.ExternalId,
				normalizedExternalType,
				request.Name,
				request.Category,
				request.Latitude,
				request.Longitude,
				request.Address,
				request.Tags,
				DateTime.UtcNow);

			await dbContext.Places.AddAsync(createdPlace, cancellationToken);
			return new PlaceUpsertResult(createdPlace.Id, true);
		}

		existingPlace.UpdateFromExternal(
			request.Name,
			request.Category,
			request.Latitude,
			request.Longitude,
			normalizedExternalType,
			request.Address,
			request.Tags,
			DateTime.UtcNow);

		return new PlaceUpsertResult(existingPlace.Id, false);
	}

	public async Task<IReadOnlyList<PlaceNearbyResult>> SearchNearbyAsync(
		double latitude,
		double longitude,
		double radiusInMeters,
		int limit,
		CancellationToken cancellationToken = default)
	{
		const double earthRadiusMeters = 6371000d;
		const double degreeToRadians = Math.PI / 180d;
		var latitudeRad = latitude * degreeToRadians;
		var cosLatitude = Math.Cos(latitudeRad);
		var sinLatitude = Math.Sin(latitudeRad);

		var results = await dbContext.Places
			.AsNoTracking()
			.Where(place => place.IsActive)
			.Select(place => new
			{
				place.Id,
				place.Name,
				place.Category,
				place.Address,
				place.Latitude,
				place.Longitude,
				place.Rating,
				place.ReviewCount,
				DistanceInMeters = earthRadiusMeters * Math.Acos(
					cosLatitude * Math.Cos(place.Latitude * degreeToRadians) *
					Math.Cos((place.Longitude - longitude) * degreeToRadians) +
					sinLatitude * Math.Sin(place.Latitude * degreeToRadians))
			})
			.Where(place => place.DistanceInMeters <= radiusInMeters)
			.OrderBy(place => place.DistanceInMeters)
			.ThenByDescending(place => place.Rating)
			.Take(limit)
			.ToListAsync(cancellationToken)
			.ConfigureAwait(false);

		return results
			.Select(place => new PlaceNearbyResult(
				place.Id,
				place.Name,
				place.Category,
				place.Address,
				place.Latitude,
				place.Longitude,
				place.Rating,
				place.ReviewCount,
				place.DistanceInMeters))
			.ToList();
	}
}

