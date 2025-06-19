using Identity.Application.Services.RefreshTokens;

namespace Identity.Api.Services;

/// <summary>
/// Background service for cleaning up expired refresh tokens
/// Background service để dọn dẹp các refresh token đã hết hạn
/// </summary>
public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefreshTokenCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Clean up every 6 hours

    public RefreshTokenCleanupService(
        IServiceProvider serviceProvider,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Refresh token cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredTokensAsync();
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Refresh token cleanup service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in refresh token cleanup service");
                // Wait before retrying
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        _logger.LogInformation("Refresh token cleanup service stopped");
    }

    private async Task CleanupExpiredTokensAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var refreshTokenService = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

            var cleanedCount = await refreshTokenService.CleanupExpiredTokensAsync();
            
            if (cleanedCount > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired refresh tokens", cleanedCount);
            }
            else
            {
                _logger.LogDebug("No expired refresh tokens to clean up");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean up expired refresh tokens");
            throw;
        }
    }
}
