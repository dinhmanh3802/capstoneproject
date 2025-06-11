// NotificationCleanupService.cs

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SCCMS.Infrastucture.Context;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class NotificationCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationCleanupService> _logger;

    public NotificationCleanupService(IServiceProvider serviceProvider, ILogger<NotificationCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Chạy định kỳ mỗi ngày một lần
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanUpOldNotifications(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up old notifications.");
            }

            // Chờ 24 giờ trước khi chạy lại
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task CleanUpOldNotifications(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cutoffDate = DateTime.Now.AddMonths(-1);

            var oldNotifications = await dbContext.Notifications
                .Where(n => n.CreatedAt < cutoffDate)
                .ToListAsync(cancellationToken);

            if (oldNotifications.Any())
            {
                dbContext.Notifications.RemoveRange(oldNotifications);
                await dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Deleted {oldNotifications.Count} old notifications.");
            }
        }
    }
}
