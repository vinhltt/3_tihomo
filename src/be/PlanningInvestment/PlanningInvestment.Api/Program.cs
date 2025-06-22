using Microsoft.EntityFrameworkCore;
using PlanningInvestment.Api;
using PlanningInvestment.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add database context
// Thêm ngữ cảnh cơ sở dữ liệu
builder.Services.AddDbContext<PlanningInvestmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(PlanningInvestmentDbContext.DEFAULT_CONNECTION_STRING))
           .UseSnakeCaseNamingConvention());

// Add health checks with DbContext
// Thêm kiểm tra sức khỏe với DbContext
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PlanningInvestmentDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();

// Add health check endpoint
app.MapHealthChecks("/health");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

namespace PlanningInvestment.Api
{
    internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}