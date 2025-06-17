using System.Text;
using Identity.Infrastructure;
using Identity.Infrastructure.Data;
using Identity.Sso.Middleware;
using Identity.Contracts.ConfigurationOptions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Get CORS configuration
var corsConfigSection = builder.Configuration.GetSection("CorsOptions");
var corsOptions = corsConfigSection.Get<CorsOptions>();
var policyName = corsOptions?.PolicyName ?? "IdentityCorsPolicy";

// Add services to the container
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation(); // Enable runtime compilation for development

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Infrastructure (database, repositories, services)
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication (for API endpoints)
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT secret key not configured");

// Authentication - Support both Cookie (SSO) and JWT (API)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        options.Cookie.Name = "TiHoMo.SSO.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    })
    .AddJwtBearer("JWT", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireUser", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("sub"));

    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin"));

    options.AddPolicy("RequireUserOrAdmin", policy =>
        policy.RequireAuthenticatedUser()
              .RequireAssertion(context =>
                  context.User.IsInRole("Admin") || 
                  context.User.HasClaim("sub", context.User.FindFirst("sub")?.Value ?? "")));
});

// OpenAPI/Swagger for API documentation
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Identity & Access Management API", 
        Version = "v1",
        Description = "API endpoints for Identity and Access Management (excludes MVC/SSO controllers)"
    });

    // Only include API controllers (those with [ApiController] attribute or routes starting with /api)
    c.DocInclusionPredicate((docName, description) =>
    {
        return description.RelativePath?.StartsWith("api/") == true;
    });

    // JWT Bearer token configuration
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }    });
});

// CORS - Use configuration-based approach
if (corsOptions != null)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(policyName, policyBuilder =>
        {
            // Configure origins
            if (corsOptions.AllowedOrigins == null || corsOptions.AllowedOrigins.Length == 0 || corsOptions.AllowedOrigins.Contains("*"))
                policyBuilder.AllowAnyOrigin();
            else
                policyBuilder.WithOrigins(corsOptions.AllowedOrigins);

            // Configure methods
            if (corsOptions.AllowedMethods == null || corsOptions.AllowedMethods.Length == 0 || corsOptions.AllowedMethods.Contains("*"))
                policyBuilder.AllowAnyMethod();
            else
                policyBuilder.WithMethods(corsOptions.AllowedMethods);

            // Configure headers
            if (corsOptions.AllowedHeaders == null || corsOptions.AllowedHeaders.Length == 0 || corsOptions.AllowedHeaders.Contains("*"))
                policyBuilder.AllowAnyHeader();
            else
                policyBuilder.WithHeaders(corsOptions.AllowedHeaders);

            // Configure exposed headers
            if (corsOptions.ExposedHeaders != null && corsOptions.ExposedHeaders.Length > 0 && !corsOptions.ExposedHeaders.Contains("*"))
                policyBuilder.WithExposedHeaders(corsOptions.ExposedHeaders);

            // Configure credentials - only if not using AllowAnyOrigin
            if (corsOptions.AllowedOrigins != null && corsOptions.AllowedOrigins.Length > 0 && !corsOptions.AllowedOrigins.Contains("*"))
            {
                policyBuilder.AllowCredentials();
            }

            // Configure preflight max age
            if (!string.IsNullOrWhiteSpace(corsOptions.PreflightMaxAgeInMinutes))
            {
                if (int.TryParse(corsOptions.PreflightMaxAgeInMinutes, out var maxAge))
                {
                    policyBuilder.SetPreflightMaxAge(TimeSpan.FromMinutes(maxAge));
                }
            }
        });
    });
}
else
{
    // Fallback to default CORS policy if configuration is missing
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(policyName, policy =>
        {
            policy.WithOrigins(
                    // Local development
                    "http://localhost:3000", "https://localhost:3000",  // Vue.js/React dev
                    "http://localhost:3001", "https://localhost:3001",  // Alternative dev
                    "http://localhost:5173", "https://localhost:5173",  // Vite dev
                    "http://localhost:8080", "https://localhost:8080",  // Alternative Vue dev
                    "http://localhost:5217", "https://localhost:5001",  // Identity.Sso
                    // Production domains
                    "https://app.tihomo.vn", "https://tihomo.vn", "https://login.tihomo.vn",
                    // Mobile development (for WebView)
                    "capacitor://localhost", "ionic://localhost",
                    "http://localhost", "https://localhost"
                  )
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .SetIsOriginAllowedToAllowWildcardSubdomains();
        });
    });
}

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<IdentityDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // OpenAPI/Swagger in development
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity & Access Management v1");
        c.RoutePrefix = "swagger"; // Serve Swagger UI at /swagger
    });
    
    // Apply migrations automatically in development
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseCors(policyName);

app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
app.UseAuthorization();

// Map routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map API controllers
app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

await app.RunAsync();
