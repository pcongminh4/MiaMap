using Application.Common.Abstractions.Data;
using MediatR;

namespace Application.Places.FindRoute;

public sealed class FindRouteQueryHandler(IRoutingRepository routingRepository) : IRequestHandler<FindRouteQuery, FindRouteResult>
{
	public async Task<FindRouteResult> Handle(FindRouteQuery request, CancellationToken cancellationToken)
	{
		return await routingRepository.FindRouteAsync(
			request.StartLatitude,
			request.StartLongitude,
			request.EndLatitude,
			request.EndLongitude,
			cancellationToken);
	}
}