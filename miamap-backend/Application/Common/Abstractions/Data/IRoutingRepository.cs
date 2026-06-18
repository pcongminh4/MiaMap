using Application.Places.FindRoute;

namespace Application.Common.Abstractions.Data;

public interface IRoutingRepository
{
	Task<FindRouteResult> FindRouteAsync(
		double startLatitude,
		double startLongitude,
		double endLatitude,
		double endLongitude,
		CancellationToken cancellationToken = default);
}