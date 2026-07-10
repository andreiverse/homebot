namespace HomeBot.Integrations.QBittorrent;

public sealed class QBittorrentIntegration
    : BaseIntegration<QBittorrentIntegration>, IMetricProvider
{
    private readonly QBittorrentHttpClient _client;

    private readonly MetricSet<TransferInfo> _metrics = new MetricSet<TransferInfo>()
        .Add("download_speed", "Current download speed (bytes/s)", d => d.DlInfoSpeed)
        .Add("upload_speed", "Current upload speed (bytes/s)", d => d.UpInfoSpeed)
        .Add("downloaded", "Total downloaded bytes", d => d.DlInfoData)
        .Add("uploaded", "Total uploaded bytes", d => d.UpInfoData)
        .Add("dht_nodes", "Connected DHT nodes", d => d.DhtNodes);

    private TransferInfo? _transferInfo;
    private DateTimeOffset _lastRefreshed;

    public QBittorrentIntegration(
        QBittorrentHttpClient client,
        ILogger<QBittorrentIntegration> logger)
        : base(
            logger,
            new IntegrationMetadata(
                "qBittorrent",
                "qBittorrent integration"))
    {
        _client = client;

        logger.LogInformation("qBittorrent integration initialized.");
    }

    IEnumerable<IIntegrationMetric> IMetricProvider.Metrics => _metrics.Metrics;

    IEnumerable<IntegrationMetricSnapshot> IMetricProvider.Snapshots =>
        _metrics.Snapshot(_transferInfo, _lastRefreshed);

    public async Task RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        _transferInfo = await _client.GetTransferInfoAsync(cancellationToken);
        _lastRefreshed = DateTimeOffset.UtcNow;
    }

    public override Task<IntegrationHealthStatus> PerformHealthCheck()
        => ProbeHealthAsync(async () =>
        {
            await _client.GetVersionAsync();
            return true;
        });

    public Task<AppVersion> GetVersionAsync(CancellationToken cancellationToken = default)
        => _client.GetVersionAsync(cancellationToken);

    public Task<BuildInfo> GetBuildInfoAsync(CancellationToken cancellationToken = default)
        => _client.GetBuildInfoAsync(cancellationToken);

    public Task<Preferences> GetPreferencesAsync(CancellationToken cancellationToken = default)
        => _client.GetPreferencesAsync(cancellationToken);

    public Task DeleteSearchAsync(int searchId, CancellationToken cancellationToken = default)
        => _client.DeleteSearchAsync(searchId, cancellationToken);

    public Task<IReadOnlyList<TorrentInfo>> GetTorrentsAsync(
        string? filter = null,
        CancellationToken cancellationToken = default)
        => _client.GetTorrentsAsync(filter, cancellationToken);

    public Task<TransferInfo> GetTransferInfoAsync(CancellationToken cancellationToken = default)
        => _client.GetTransferInfoAsync(cancellationToken);
}
