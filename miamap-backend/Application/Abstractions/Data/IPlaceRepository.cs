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
}

