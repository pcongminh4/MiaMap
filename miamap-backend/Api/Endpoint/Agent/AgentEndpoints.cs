using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Application.Agent.Chat;
using Application.Agent.SearchImage;
using Application.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Api.Endpoint.Agent;

public sealed class AgentEndpoints : IEndPoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("/agent/chat", async (
			AgentChatRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
			{
				var query = new AgentChatQuery(
					request.Prompt,
					request.Latitude,
					request.Longitude);

				var response = await sender.Send(query, cancellationToken);

				return Results.Ok(response);
			})
			.WithTags(Tags.Agent)
			.WithName("AgentChat")
			.WithSummary("Chats with AI Agent assistant")
			.WithDescription("Sends a prompt with current location coordinates to get an AI agent chat response including map actions.")
			.Produces<AgentChatResult>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError)
			.DisableAntiforgery();

		app.MapPost("/agent/search-image", async (
			IFormFile file,
			[FromQuery] double? lat,
			[FromQuery] double? lng,
			ISender sender,
			CancellationToken cancellationToken) =>
			{
				if (file is null || file.Length == 0)
				{
					return Results.BadRequest("No image file uploaded.");
				}

				using var stream = file.OpenReadStream();
				var query = new AgentSearchImageQuery(
					stream,
					lat ?? 10.7712,
					lng ?? 106.6980);

				var response = await sender.Send(query, cancellationToken);

				return Results.Ok(response);
			})
			.WithTags(Tags.Agent)
			.WithName("AgentSearchImage")
			.WithSummary("Recognizes landmark/shop from an uploaded image")
			.WithDescription("Uploads an image with current location to search/identify map places using multimodal AI.")
			.Produces<AgentSearchImageResult>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError)
			.DisableAntiforgery();
	}
}

public sealed record AgentChatRequest(
	string Prompt,
	double Latitude,
	double Longitude);
