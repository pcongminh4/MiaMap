using Application.Places.GetActiveReports;
using MediatR;

namespace Api.Endpoint.Places;

public sealed class GetActiveReportsEndPoint : IEndPoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/reports/active", async (
            [AsParameters] GetActiveReportsQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var results = await sender.Send(query, cancellationToken);
            return Results.Ok(results);
        })
        .WithTags(Tags.Places)
        .WithName("GetActiveReports")
        .WithSummary("Gets active traffic reports inside bounding box")
        .Produces<IReadOnlyList<ActiveReportResult>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
