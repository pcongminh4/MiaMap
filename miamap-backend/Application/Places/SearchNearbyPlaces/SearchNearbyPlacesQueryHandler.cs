using Application.Abstractions.Data;
using Application.Results;
using MediatR;

namespace Application.Places.SearchNearbyPlaces;

public sealed class SearchNearbyPlacesQueryHandler(IPlaceRepository placeRepository)
	: IRequestHandler<SearchNearbyPlacesQuery, IReadOnlyList<PlaceNearbyResult>>
{
	public async Task<IReadOnlyList<PlaceNearbyResult>> Handle(SearchNearbyPlacesQuery request, CancellationToken cancellationToken)
	{
		var places = await placeRepository.SearchNearbyAsync(
			request.Latitude,
			request.Longitude,
			request.RadiusInMeters,
			request.Limit,
			cancellationToken);

		return places;
	}
}

