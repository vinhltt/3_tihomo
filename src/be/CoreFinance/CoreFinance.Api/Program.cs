using CoreFinance.Api.Infrastructures.Middlewares;
using CoreFinance.Api.Infrastructures.Modules;
using CoreFinance.Api.Infrastructures.ServicesExtensions;
using Shared.Contracts.ConfigurationOptions;
using Shared.Contracts.Utilities;
using CoreFinance.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

async Task CreateDbIfNotExistsAsync(IHost host)
{
    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<CoreFinanceDbContext>();
        await context.Database.MigrateAsync();
        //var dbInitializer = services.GetService<DbInitializer>();
        //if (dbInitializer == null)
        //{
        //    logger.LogError("dbInitializer is null");
        //    return;
        //}

        //await dbInitializer.Seed();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{env}.json", true, true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

var corsOption = configuration.GetOptions<CorsOptions>("CorsOptions");
var policyName = corsOption!.PolicyName.Nullify("AppCorsPolicy");
builder.AddConfigurationSettings();
builder.AddGeneralConfigurations(policyName, corsOption);
builder.Services.AddInjectedServices();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CoreFinanceDbContext>();
builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(hostingContext.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName();
});

var app = builder.Build();

// Add the performance logging middleware early in the pipeline
app.UseMiddleware<PerformanceLoggingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreFinance Api v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

app.UseCors(policyName);

// Add the authorization middleware
//app.UseAuthorization();
app.UseRouting();

// Map health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

await CreateDbIfNotExistsAsync(app);


try
{
    await app.RunAsync();
}
finally
{
    await Log.CloseAndFlushAsync();
}