using BackEnd.Modules.Database;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Modules.Auth
{
    public class BlacklistCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<BlacklistCleanupService> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<BlacklistCleanupService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var expired = await db.BlacklistTokens
                    .Where(x => x.ExpiryTime <= DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                if (expired.Count > 0)
                {
                    db.BlacklistTokens.RemoveRange(expired);
                    await db.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Removed {Count} expired blacklist tokens.", expired.Count);
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}