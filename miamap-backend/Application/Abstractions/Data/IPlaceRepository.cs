using Domain.Places;

namespace Application.Abstractions.Data;

public interface IPlaceRepository
{
	Task AddAsync(Place place, CancellationToken cancellationToken = default);

	Task<PlaceUpsertResult> UpsertExternalAsync(
		ExternalPlaceUpsertRequest request,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<PlaceNearbyResult>> SearchNearbyAsync(
		double latitude,
		double longitude,
		double radiusInMeters,
		int limit,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<BoudingBoxResult>> BoudingBoxSearchAsync(
		double minLatitude,
		double minLongitude,
		double maxLatitude,
		double maxLongitude,
		int limit,
		CancellationToken cancellationToken = default);
}

