using Microsoft.Extensions.Options;

namespace HomeBot.Integrations.Prometheus;

public sealed class PrometheusIntegration : BaseIntegration<PrometheusIntegration>, IMetricProvider
{
    private readonly PrometheusHttpClient _client;
    private readonly IOptionsMonitor<PrometheusOptions> _options;
    private readonly List<IIntegrationMetric> _metrics;

    public PrometheusIntegration(
        PrometheusHttpClient client,
        IOptionsMonitor<PrometheusOptions> options,
        ILogger<PrometheusIntegration> logger)
        : base(
            logger,
            new IntegrationMetadata(
                "Prometheus",
                "Prometheus monitoring integration"))
    {
        _client = client;
        _options = options;

        logger.LogInformation("Prometheus integration initialized.");

        _metrics = _options.CurrentValue.Queries
            .Select(q => new IntegrationMetric(q.Name))
            .Cast<IIntegrationMetric>()
            .ToList();
    }

    private readonly Dictionary<string, IntegrationMetricSnapshot> _snapshots = [];
    IEnumerable<IIntegrationMetric> IMetricProvider.Metrics => _metrics;
    IEnumerable<IntegrationMetricSnapshot> IMetricProvider.Snapshots => _snapshots.Values;

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _metrics.Select(async metric =>
        {
            var query = _options.CurrentValue.Queries
                .First(q => q.Name == metric.Name);

            var response = await QueryAsync(query.PromQL, cancellationToken);

            object? value = response.Data.Result
                .FirstOrDefault()?
                .Value[1];

            _snapshots[metric.Id] = new IntegrationMetricSnapshot(
                metric,
                value,
                DateTimeOffset.UtcNow);
        });

        await Task.WhenAll(tasks);
    }

    public override Task<IntegrationHealthStatus> PerformHealthCheck()
        => ProbeHealthAsync(async () =>
        {
            if (!await _client.IsHealthyAsync())
                return false;

            var runtime = await _client.GetRuntimeInfoAsync();

            return runtime.Status == "success";
        });

    public Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
        => _client.IsHealthyAsync(cancellationToken);

    public Task<PrometheusRuntimeInfoResponse> GetRuntimeInfoAsync(CancellationToken cancellationToken = default)
        => _client.GetRuntimeInfoAsync(cancellationToken);

    public Task<PrometheusTargetsResponse> GetTargetsAsync(CancellationToken cancellationToken = default)
        => _client.GetTargetsAsync(cancellationToken);

    public Task<PrometheusAlertsResponse> GetAlertsAsync(CancellationToken cancellationToken = default)
        => _client.GetAlertsAsync(cancellationToken);

    public Task<PrometheusQueryResponse> QueryAsync(string promQl, CancellationToken cancellationToken = default)
        => _client.QueryAsync(promQl, cancellationToken);

    public Task<PrometheusRangeQueryResponse> QueryRangeAsync(
        string promQl,
        DateTimeOffset start,
        DateTimeOffset end,
        TimeSpan step,
        CancellationToken cancellationToken = default)
        => _client.QueryRangeAsync(promQl, start, end, step, cancellationToken);
}
