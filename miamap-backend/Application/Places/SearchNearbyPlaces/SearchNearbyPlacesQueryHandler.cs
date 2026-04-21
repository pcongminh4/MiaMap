using Application.Abstractions.Data;
using Application.Places;
using MediatR;

namespace Application.Places.SearchNearbyPlaces;

public sealed class SearchNearbyPlacesQueryHandler(IPlaceRepository placeRepository)
	: IRequestHandler<SearchNearbyPlacesQuery, IReadOnlyList<PlaceNearbyResponse>>
{
	public async Task<IReadOnlyList<PlaceNearbyResponse>> Handle(SearchNearbyPlacesQuery request, CancellationToken cancellationToken)
	{
		var places = await placeRepository.SearchNearbyAsync(
			request.Latitude,
			request.Longitude,
			request.RadiusInMeters,
			request.Limit,
			cancellationToken);

		return places
			.Select(place => new PlaceNearbyResponse(
				place.PlaceId,
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

