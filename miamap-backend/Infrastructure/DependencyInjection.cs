using Application.Common.Abstractions.Authentication;
using Application.Common.Abstractions.Data;
using Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Database;
using Infrastructure.Places.Importing;
using Infrastructure.Places;
using Infrastructure.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Default' is not configured.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseNetTopologySuite()));

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IPlaceRepository, PlaceRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoutingRepository, RoutingRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, TokenProvider>();

        services.Configure<OpenStreetMapImportOptions>(
            configuration.GetSection(OpenStreetMapImportOptions.SectionName));
        services.AddHttpClient("openstreetmap-import");
        services.AddHostedService<OpenStreetMapImportBackgroundService>();

        return services;
    }
}

