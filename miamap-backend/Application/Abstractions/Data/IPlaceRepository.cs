using Application.Results;
using Domain.Places;


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

	Task<IReadOnlyList<BoundingBoxResult>> BoudingBoxSearchAsync(
		double minLatitude,
		double minLongitude,
		double maxLatitude,
		double maxLongitude,
		int limit,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<SearchByNameOrAddressResult>> SearchByNameOrAddressAsync(
		string searchText,
		int limit,
		CancellationToken cancellationToken = default);
}

