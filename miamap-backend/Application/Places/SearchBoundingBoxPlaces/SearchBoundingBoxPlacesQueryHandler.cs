using Application.Results;
using MediatR;

namespace Application.Places.SearchBoundingBoxPlaces;

public sealed class SearchBoundingBoxPlacesQueryHandler(IPlaceRepository placeRepository)
	: IRequestHandler<SearchBoundingBoxPlacesQuery, IReadOnlyList<BoundingBoxResult>>
{
	public async Task<IReadOnlyList<BoundingBoxResult>> Handle(
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
