using Api.Endpoint;

namespace Api.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var endpoints = scope.ServiceProvider.GetServices<IEndPoint>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}