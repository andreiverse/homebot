using Microsoft.Extensions.Options;

namespace HomeBot.Integrations.Jellyfin;

public sealed class JellyfinIntegration
    : BaseIntegration<JellyfinIntegration>, IMetricProvider
{
    private readonly JellyfinHttpClient _client;

    private ItemCounts? _itemCounts;

    private readonly IIntegrationMetric _episodesMetric =
        new IntegrationMetric("episodes", "Total Episodes");

    private readonly IIntegrationMetric _moviesMetric =
        new IntegrationMetric("movies", "Total Movies");

    private readonly IIntegrationMetric _seriesMetric =
        new IntegrationMetric("series", "Total Series");

    public JellyfinIntegration(
        IOptionsMonitor<JellyfinOptions> options,
        ILogger<JellyfinIntegration> logger)
        : base(
            logger,
            new IntegrationMetadata(
                "Jellyfin",
                "Jellyfin integration"))
    {
        _client = new JellyfinHttpClient(
            options.CurrentValue.Endpoint,
            options.CurrentValue.ApiKey);

        logger.LogInformation("Jellyfin integration initialized.");
    }

    IEnumerable<IIntegrationMetric> IMetricProvider.Metrics =>
    [
        _episodesMetric,
        _moviesMetric,
        _seriesMetric
    ];

    IEnumerable<IntegrationMetricSnapshot> IMetricProvider.Snapshots =>
    [
        new(_episodesMetric, _itemCounts?.EpisodeCount, DateTimeOffset.UtcNow),
        new(_moviesMetric, _itemCounts?.MovieCount, DateTimeOffset.UtcNow),
        new(_seriesMetric, _itemCounts?.SeriesCount, DateTimeOffset.UtcNow)
    ];

    public async Task RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        _itemCounts = await _client.GetItemCountsAsync();
    }

    public override async Task<IntegrationHealthStatus> PerformHealthCheck()
    {
        try
        {
            var sysInfo = await _client.GetSystemInfoAsync();

            return !sysInfo.IsShuttingDown
                ? IntegrationHealthStatus.Healthy
                : IntegrationHealthStatus.Unhealthy;
        }
        catch
        {
            return IntegrationHealthStatus.Unhealthy;
        }
    }

    public Task<SystemInfo> GetSystemInfoAsync()
        => _client.GetSystemInfoAsync();

    public Task<ItemCounts> GetItemCountsAsync()
        => _client.GetItemCountsAsync();
}