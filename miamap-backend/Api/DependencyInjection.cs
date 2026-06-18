using Api.Endpoint;
using Api.Middleware;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Api;

public static class DependencyInjection
{
    public const string CorsPolicyName = "DefaultCorsPolicy";
    public const string AuthCookieName = "access_token";
    public const string RefreshCookieName = "refresh_token";

    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        var jwtSection = configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var secret = jwtSection["Secret"];

        if (string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience) ||
            string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("Jwt configuration is missing Issuer, Audience, or Secret.");
        }

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    return;
                }

                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue(AuthCookieName, out var cookieToken) &&
                            !string.IsNullOrWhiteSpace(cookieToken))
                        {
                            context.Token = cookieToken;
                        }

                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        var assembly = typeof(DependencyInjection).Assembly;

        var endpointTypes = assembly
            .DefinedTypes
            .Where(type => !type.IsAbstract && !type.IsInterface && typeof(IEndPoint).IsAssignableFrom(type))
            .ToArray();

        foreach (var endpointType in endpointTypes)
        {
            services.AddScoped(typeof(IEndPoint), endpointType);
        }

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddExceptionHandler<UnhandledExceptionHandler>();
        services.AddProblemDetails();

        services.AddAuthorization();
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "XSRF-TOKEN";
            options.Cookie.HttpOnly = false;
        });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}
