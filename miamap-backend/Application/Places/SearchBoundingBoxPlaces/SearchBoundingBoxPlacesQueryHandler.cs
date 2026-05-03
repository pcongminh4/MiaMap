using Application.Abstractions.Data;
using Application.Places;
using MediatR;

namespace Application.Places.SearchBoundingBoxPlaces;

public sealed class SearchBoundingBoxPlacesQueryHandler(IPlaceRepository placeRepository)
	: IRequestHandler<SearchBoundingBoxPlacesQuery, IReadOnlyList<BoundingBoxPlaceResponse>>
{
	public async Task<IReadOnlyList<BoundingBoxPlaceResponse>> Handle(
		SearchBoundingBoxPlacesQuery request,
		CancellationToken cancellationToken)
	{
		var places = await placeRepository.BoudingBoxSearchAsync(
			request.MinLatitude,
			request.MinLongitude,
			request.MaxLatitude,
			request.MaxLongitude,
			request.Limit,
			cancellationToken);

		return places
			.Select(place => new BoundingBoxPlaceResponse(
				place.PlaceId,
				place.Name,
				place.Category,
				place.Address,
				place.Latitude,
				place.Longitude,
				place.Rating,
				place.ReviewCount))
			.ToList();
	}
}
