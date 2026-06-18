using System.Security.Claims;
using Application.Places.CreateReport;
using Infrastructure.Authentication;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;

namespace Api.Endpoint.Places;

public sealed class CreateReportEndPoint : IEndPoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/reports", async (
            HttpContext httpContext,
            CreateReportRequest requestBody,
            ClaimsPrincipal claimsPrincipal,
            ISender sender,
            IAntiforgery antiforgery,
            CancellationToken cancellationToken) =>
        {
            await antiforgery.ValidateRequestAsync(httpContext);

            var userId = claimsPrincipal.GetUserId();
            var command = new CreateReportCommand(
                userId,
                requestBody.ReportType,
                requestBody.SubType,
                requestBody.Latitude,
                requestBody.Longitude,
                requestBody.Description);

            var reportId = await sender.Send(command, cancellationToken);
            return Results.Created($"/reports/{reportId}", reportId);
        })
        .RequireAuthorization()
        .WithTags(Tags.Places)
        .WithName("CreateReport")
        .WithSummary("Creates a traffic report")
        .Produces<int>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}

public sealed record CreateReportRequest(
    string ReportType,
    string? SubType,
    double Latitude,
    double Longitude,
    string? Description);
