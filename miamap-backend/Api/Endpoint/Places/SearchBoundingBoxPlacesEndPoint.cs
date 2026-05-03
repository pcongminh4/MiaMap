using Api.Endpoint;
using Application.Places;
using Application.Places.SearchBoundingBoxPlaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoint.Places;

public sealed class SearchBoundingBoxPlacesEndPoint : IEndPoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("/places/bounding-box", async ([AsParameters] SearchBoundingBoxPlacesRequest request, ISender sender, CancellationToken cancellationToken) =>
			{
				var query = new SearchBoundingBoxPlacesQuery(
					request.MinLatitude,
					request.MinLongitude,
					request.MaxLatitude,
					request.MaxLongitude,
					request.Limit);

				var response = await sender.Send(query, cancellationToken);
				return Results.Ok(response);
			})
			.WithTags(Tags.Places)
			.WithName("SearchBoundingBoxPlaces")
			.WithSummary("Searches places in a bounding box")
			.WithDescription("Returns active places in the map bounds sorted by rating and review count.")
			.Produces<IReadOnlyList<BoundingBoxPlaceResponse>>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
	}
}
