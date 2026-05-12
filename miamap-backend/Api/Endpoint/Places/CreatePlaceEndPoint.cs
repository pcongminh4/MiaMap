using Application.Places.CreatePlace;
using MediatR;

namespace Api.Endpoint.Places;


public sealed class CreatePlaceEndPoint : IEndPoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("/places", async (
			CreatePlaceCommand request, 
			ISender sender, 
			CancellationToken cancellationToken) =>
			{
				var placeId = await sender.Send(request, cancellationToken);
				return Results.Created($"/places/{placeId}", placeId);
			})
			.WithTags(Tags.Places)
			.WithName("CreatePlace")
			.WithSummary("Creates a place")
			.WithDescription("Creates a new place for location search.")
			.Produces<int>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
	}
}
