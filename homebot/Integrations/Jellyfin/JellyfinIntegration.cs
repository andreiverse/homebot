namespace HomeBot.Integrations.Jellyfin;

public sealed class JellyfinIntegration
    : BaseIntegration<JellyfinIntegration>, IMetricProvider
{
    private readonly JellyfinHttpClient _client;

    private readonly MetricSet<ItemCounts> _metrics = new MetricSet<ItemCounts>()
        .Add("episodes", "Total Episodes", d => d.EpisodeCount)
        .Add("movies", "Total Movies", d => d.MovieCount)
        .Add("series", "Total Series", d => d.SeriesCount);

    private ItemCounts? _itemCounts;
    private DateTimeOffset _lastRefreshed;

    public JellyfinIntegration(
        JellyfinHttpClient client,
        ILogger<JellyfinIntegration> logger)
        : base(
            logger,
            new IntegrationMetadata(
                "Jellyfin",
                "Jellyfin integration"))
    {
        _client = client;

        logger.LogInformation("Jellyfin integration initialized.");
    }

    IEnumerable<IIntegrationMetric> IMetricProvider.Metrics => _metrics.Metrics;

    IEnumerable<IntegrationMetricSnapshot> IMetricProvider.Snapshots =>
        _metrics.Snapshot(_itemCounts, _lastRefreshed);

    public async Task RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        _itemCounts = await _client.GetItemCountsAsync(cancellationToken);
        _lastRefreshed = DateTimeOffset.UtcNow;
    }

    public override Task<IntegrationHealthStatus> PerformHealthCheck()
        => ProbeHealthAsync(async () =>
        {
            var sysInfo = await _client.GetSystemInfoAsync();
            return !sysInfo.IsShuttingDown;
        });

    public Task<SystemInfo> GetSystemInfoAsync(CancellationToken cancellationToken = default)
        => _client.GetSystemInfoAsync(cancellationToken);

    public Task<ItemCounts> GetItemCountsAsync(CancellationToken cancellationToken = default)
        => _client.GetItemCountsAsync(cancellationToken);
}
