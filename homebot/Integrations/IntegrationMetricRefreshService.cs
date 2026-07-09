namespace HomeBot.Integrations;

public sealed class IntegrationMetricRefreshService : BackgroundService
{
    private readonly IntegrationMetricManager _manager;

    public IntegrationMetricRefreshService(
        IntegrationMetricManager manager)
    {
        _manager = manager;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        do
        {
            await _manager.RefreshAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}