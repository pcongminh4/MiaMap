using Application.Abstractions.Data;
using MediatR;

namespace Application.Places.SearchBoundingBoxPlaces;

public sealed class SearchBoundingBoxPlacesQueryHandler(IPlaceRepository placeRepository)
	: IRequestHandler<SearchBoundingBoxPlacesQuery, IReadOnlyList<BoudingBoxResult>>
{
	public async Task<IReadOnlyList<BoudingBoxResult>> Handle(
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

		return places;
	}
}
