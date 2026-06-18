using System.Security.Claims;
using Application.Places.VoteReport;
using Infrastructure.Authentication;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;

namespace Api.Endpoint.Places;

public sealed class VoteReportEndPoint : IEndPoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/reports/{id}/vote", async (
            int id,
            VoteReportRequest requestBody,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            ISender sender,
            IAntiforgery antiforgery,
            CancellationToken cancellationToken) =>
        {
            await antiforgery.ValidateRequestAsync(httpContext);

            var userId = claimsPrincipal.GetUserId();
            var command = new VoteReportCommand(id, userId, requestBody.IsUpvote);

            await sender.Send(command, cancellationToken);
            return Results.Ok(new { message = "Vote recorded successfully." });
        })
        .RequireAuthorization()
        .WithTags(Tags.Places)
        .WithName("VoteReport")
        .WithSummary("Votes on a traffic report")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}

public sealed record VoteReportRequest(bool IsUpvote);
