using Application.Places.SearchNearbyPlaces;
using Application.Results;
using MediatR;

namespace Api.Endpoint.Places;

public sealed class SearchNearbyPlacesEndPoint : IEndPoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("/places/nearby", async ([AsParameters] SearchNearbyPlacesQuery request, ISender sender, CancellationToken cancellationToken) =>
			{
				var response = await sender.Send(request, cancellationToken);
				return Results.Ok(response);
			})
			.WithTags(Tags.Places)
			.WithName("SearchNearbyPlaces")
			.WithSummary("Searches nearby places")
			.WithDescription("Returns active places within radius sorted by distance then rating.")
			.Produces<PlaceNearbyResult>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
	}
}
