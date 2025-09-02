using Microsoft.EntityFrameworkCore;
using PlanningInvestment.Api;
using PlanningInvestment.Infrastructure;
using PlanningInvestment.Application.Interfaces;
using PlanningInvestment.Application.Services;
using PlanningInvestment.Application.Profiles;
using PlanningInvestment.Domain.UnitOfWorks;
using PlanningInvestment.Infrastructure.UnitOfWorks;
using PlanningInvestment.Domain.BaseRepositories;
using PlanningInvestment.Infrastructure.Repositories.Base;

async Task MigrateDatabaseAsync(IHost host)
{
    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<PlanningInvestmentDbContext>();
        await context.Database.MigrateAsync();
        logger.LogInformation("PlanningInvestment database migration completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the PlanningInvestment database");
    }
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add controllers
builder.Services.AddControllers();

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(InvestmentProfile));

// Add database context
// Thêm ngữ cảnh cơ sở dữ liệu
builder.Services.AddDbContext<PlanningInvestmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(PlanningInvestmentDbContext.DEFAULT_CONNECTION_STRING))
           .UseSnakeCaseNamingConvention());

// Register repositories
// Đăng ký các repository
builder.Services.AddScoped(typeof(IBaseRepository<,>), typeof(BaseRepository<,>));

// Register Unit of Work
// Đăng ký Unit of Work
builder.Services.AddScoped<IUnitOfWork>(provider => 
    new UnitOfWork<PlanningInvestmentDbContext>(
        provider.GetRequiredService<PlanningInvestmentDbContext>(),
        provider
    ));

// Register application services
// Đăng ký các dịch vụ ứng dụng
builder.Services.AddScoped<IInvestmentService, InvestmentService>();

// Add health checks with DbContext
// Thêm kiểm tra sức khỏe với DbContext
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PlanningInvestmentDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();

// Add authentication and authorization middleware
// Thêm middleware xác thực và ủy quyền
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
// Ánh xạ controllers
app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

// ✅ Migrate database on startup
// Tự động migrate database khi khởi động
await MigrateDatabaseAsync(app);

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
    public partial class Program { } // For integration testing

    internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}