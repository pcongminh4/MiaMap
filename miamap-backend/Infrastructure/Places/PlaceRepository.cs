using Application.Common.Abstractions.Data;
using Application.Results;
using Domain.Places;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

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
		var searchPoint = new Point(longitude, latitude) { SRID = 4326 };

		var results = await dbContext.Places
			.AsNoTracking()
			.Where(place => place.IsActive)
			.Select(place => new
			{
				place.Id,
				place.Name,
				place.Category,
				place.Address,
				Location = new GeoPoint(place.Point.Y, place.Point.X),
				place.Rating,
				place.ReviewCount,
				DistanceInMeters = place.Point.Distance(searchPoint)
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
				place.Location,
				place.Rating,
				place.ReviewCount,
				place.DistanceInMeters))
			.ToList();
	}

	public async Task<IReadOnlyList<BoundingBoxResult>> BoudingBoxSearchAsync(
		double minLatitude,
		double minLongitude,
		double maxLatitude,
		double maxLongitude,
		int limit,
		CancellationToken cancellationToken = default)
	{
		var envelope = new Polygon(new LinearRing(new[]
		{
			new Coordinate(minLongitude, minLatitude),
			new Coordinate(minLongitude, maxLatitude),
			new Coordinate(maxLongitude, maxLatitude),
			new Coordinate(maxLongitude, minLatitude),
			new Coordinate(minLongitude, minLatitude)
		}))
		{
			SRID = 4326
		};

		var results = await dbContext.Places
			.AsNoTracking()
			.Where(place => place.IsActive)
			.Where(place => place.Point.Within(envelope))
			.OrderByDescending(place => place.Rating)
			.ThenByDescending(place => place.ReviewCount)
			.Take(limit)
			.Select(place => new BoundingBoxResult(
				place.Id,
				place.Name,
				place.Category,
				place.Address,
				new GeoPoint(place.Point.Y, place.Point.X),
				place.Rating,
				place.ReviewCount))
			.ToListAsync(cancellationToken)
			.ConfigureAwait(false);

		return results;
	}

	public async Task<IReadOnlyList<SearchByNameOrAddressResult>> SearchByNameOrAddressAsync(
	string searchText,
	int limit,
	CancellationToken cancellationToken = default)
	{
		var searchWords = searchText
			.Split(' ', StringSplitOptions.RemoveEmptyEntries)
			.Select(word => word.Trim())
			.Where(word => !string.IsNullOrWhiteSpace(word))
			.ToList();

		if (searchWords.Count == 0)
		{
			return [];
		}

		IQueryable<Place> query = dbContext.Places
			.AsNoTracking()
			.Where(place => place.IsActive);

		foreach (var word in searchWords)
		{
			var pattern = "%" + word + "%";

			query = query.Where(place =>
				EF.Functions.ILike(place.Name, pattern) ||
				(place.Address != null &&
				 EF.Functions.ILike(place.Address, pattern)));
		}

		var places = await query
			.Take(limit)
			.Select(place => new SearchByNameOrAddressResult(
				place.Id,
				place.Name,
				place.Category,
				place.Address,
				new GeoPoint(place.Point.Y, place.Point.X),
				place.Rating,
				place.ReviewCount))
			.ToListAsync(cancellationToken);

		return places;
	}
}

