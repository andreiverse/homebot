using Microsoft.Extensions.Options;

namespace HomeBot.Integrations.QBittorrent;

public sealed class QBittorrentIntegration
    : BaseIntegration<QBittorrentIntegration>, IMetricProvider
{

    private readonly QBittorrentHttpClient _client;

    private TransferInfo? _transferInfo;

    private readonly IIntegrationMetric _downloadSpeedMetric =
        new IntegrationMetric("download_speed", "Current download speed (bytes/s)");

    private readonly IIntegrationMetric _uploadSpeedMetric =
        new IntegrationMetric("upload_speed", "Current upload speed (bytes/s)");

    private readonly IIntegrationMetric _downloadedMetric =
        new IntegrationMetric("downloaded", "Total downloaded bytes");

    private readonly IIntegrationMetric _uploadedMetric =
        new IntegrationMetric("uploaded", "Total uploaded bytes");

    private readonly IIntegrationMetric _dhtNodesMetric =
        new IntegrationMetric("dht_nodes", "Connected DHT nodes");

    public QBittorrentIntegration(
        IOptionsMonitor<QBittorrentOptions> options,
        ILogger<QBittorrentIntegration> logger)
        : base(
            logger,
            new IntegrationMetadata(
                "qBittorrent",
                "qBittorrent integration"))
    {
        _client = new QBittorrentHttpClient(
            options.CurrentValue.Endpoint,
            "",
            "");

        logger.LogInformation("qBittorrent integration initialized.");
    }

    IEnumerable<IIntegrationMetric> IMetricProvider.Metrics =>
    [
        _downloadSpeedMetric,
        _uploadSpeedMetric,
        _downloadedMetric,
        _uploadedMetric,
        _dhtNodesMetric
    ];

    IEnumerable<IntegrationMetricSnapshot> IMetricProvider.Snapshots =>
    [
        new(_downloadSpeedMetric, _transferInfo?.DlInfoSpeed, DateTimeOffset.UtcNow),
        new(_uploadSpeedMetric, _transferInfo?.UpInfoSpeed, DateTimeOffset.UtcNow),
        new(_downloadedMetric, _transferInfo?.DlInfoData, DateTimeOffset.UtcNow),
        new(_uploadedMetric, _transferInfo?.UpInfoData, DateTimeOffset.UtcNow),
        new(_dhtNodesMetric, _transferInfo?.DhtNodes, DateTimeOffset.UtcNow)
    ];

    public async Task RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        _transferInfo = await _client.GetTransferInfoAsync();
    }


    public override async Task<IntegrationHealthStatus> PerformHealthCheck()
    {
        try
        {
            await _client.GetVersionAsync();
            return IntegrationHealthStatus.Healthy;
        }
        catch
        {
            return IntegrationHealthStatus.Unhealthy;
        }
    }

    public Task<AppVersion> GetVersionAsync()
        => _client.GetVersionAsync();

    public Task<BuildInfo> GetBuildInfoAsync()
        => _client.GetBuildInfoAsync();

    public Task<Preferences> GetPreferencesAsync()
        => _client.GetPreferencesAsync();
    
    public Task DeleteSearchAsync(int searchId)
        => _client.DeleteSearchAsync(searchId);

    public Task<IReadOnlyList<TorrentInfo>> GetTorrentsAsync(string? filter = null)
        => _client.GetTorrentsAsync(filter);

    public Task<TransferInfo> GetTransferInfoAsync()
        => _client.GetTransferInfoAsync();
}