using Application.Places.SearchBoundingBoxPlaces;
using MediatR;
using Application.Common.Abstractions.Data;
using Application.Results;

namespace Api.Endpoint.Places;

public sealed class SearchBoundingBoxPlacesEndPoint : IEndPoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("/places/bounding-box", async (
			[AsParameters] SearchBoundingBoxPlacesQuery request, 
			ISender sender, 
			CancellationToken cancellationToken) =>
			{
				var response = await sender.Send(request, cancellationToken);
				return Results.Ok(response);
			})
			.WithTags(Tags.Places)
			.WithName("SearchBoundingBoxPlaces")
			.WithSummary("Searches places in a bounding box")
			.WithDescription("Returns active places in the map bounds sorted by rating and review count.")
			.Produces<BoundingBoxResult>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
	}
}
