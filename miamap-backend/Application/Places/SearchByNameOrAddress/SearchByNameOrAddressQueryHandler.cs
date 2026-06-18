using Application.Common.Abstractions.Data;
using Application.Results;
using MediatR;

namespace Application.Places.SearchByNameOrAddress;

public sealed class SearchByNameOrAddressQueryHandler(IPlaceRepository placeRepository)
	: IRequestHandler<SearchByNameOrAddressQuery, IReadOnlyList<SearchByNameOrAddressResult>>
{
	public async Task<IReadOnlyList<SearchByNameOrAddressResult>> Handle(
		SearchByNameOrAddressQuery request,
		CancellationToken cancellationToken)
	{
		var places = await placeRepository.SearchByNameOrAddressAsync(
			request.SearchText,
			request.Limit,
			cancellationToken);

		return places;
	}
}