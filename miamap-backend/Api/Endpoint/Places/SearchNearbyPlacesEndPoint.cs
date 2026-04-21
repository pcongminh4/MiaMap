using Api.Endpoint;
using Application.Places;
using Application.Places.SearchNearbyPlaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoint.Places;

public sealed class SearchNearbyPlacesEndPoint : IEndPoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("/places/nearby", async ([AsParameters] SearchNearbyPlacesRequest request, ISender sender, CancellationToken cancellationToken) =>
			{
				var query = new SearchNearbyPlacesQuery(
					request.Latitude,
					request.Longitude,
					request.RadiusInMeters,
					request.Limit);

				var response = await sender.Send(query, cancellationToken);
				return Results.Ok(response);
			})
			.WithTags(Tags.Places)
			.WithName("SearchNearbyPlaces")
			.WithSummary("Searches nearby places")
			.WithDescription("Returns active places within radius sorted by distance then rating.")
			.Produces<IReadOnlyList<PlaceNearbyResponse>>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
	}
}
