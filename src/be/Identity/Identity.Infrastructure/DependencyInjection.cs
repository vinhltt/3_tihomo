using Identity.Application.Common.Interfaces;
using Identity.Application.Services.ApiKeys;
using Identity.Application.Services.Authentication;
using Identity.Application.Services.Roles;
using Identity.Application.Services.Users;
using Identity.Domain.Repositories;
using Identity.Infrastructure.Data;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database - Use InMemory for development if PostgreSQL is not available
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var useInMemory = configuration.GetValue("UseInMemoryDatabase", false);

        if (useInMemory)
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseInMemoryDatabase("IdentityDb"));
        else
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Infrastructure Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IApiKeyHasher, ApiKeyHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();

        // OpenIddict configuration for SSO
        services.AddOpenIddict()
            // Register the OpenIddict core components.
            .AddCore(options =>
            {
                // Configure OpenIddict to use the Entity Framework Core stores and models.
                options.UseEntityFrameworkCore()
                    .UseDbContext<IdentityDbContext>();
            })
            // Register the OpenIddict server components.
            .AddServer(options =>
            {
                // Enable the authorization and token endpoints
                options.SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token");

                // Mark the "email", "profile" and "roles" scopes as supported scopes.
                options.RegisterScopes("email", "profile", "roles",
                    "offline_access"); // Note: this sample only uses the authorization code flow but you can enable
                // the other flows if you need to support implicit, password or client credentials.
                options.AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow(); // Register the signing and encryption credentials.
                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate(); // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableStatusCodePagesIntegration()
                    .DisableTransportSecurityRequirement(); // Allow HTTP in development

                // Note: DisableTransportSecurityRequirement allows HTTP for development only
            })

            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });

        return services;
    }
}