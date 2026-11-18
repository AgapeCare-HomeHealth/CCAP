using CCAP.Application.Abstractions.Scheduling;

namespace CCAP.API.Services;

public sealed class CalendarImportBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CalendarImportBackgroundService> _logger;

    public CalendarImportBackgroundService(IServiceScopeFactory scopeFactory, ILogger<CalendarImportBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<ICalendarImportProcessor>();
                var processed = await processor.ProcessNextBatchAsync(stoppingToken);
                if (processed) continue;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Calendar import worker iteration failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
    }
}
