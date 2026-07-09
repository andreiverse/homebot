using Microsoft.Extensions.Options;

namespace HomeBot.Integrations.Prometheus;

public sealed class PrometheusIntegration : BaseIntegration<PrometheusIntegration>, IMetricProvider
{
    private readonly PrometheusHttpClient _client;
    private readonly IOptionsMonitor<PrometheusOptions> _options;


    public PrometheusIntegration(
        IOptionsMonitor<PrometheusOptions> options,
        ILogger<PrometheusIntegration> logger)
        : base(
            logger,
            new IntegrationMetadata(
                "Prometheus",
                "Prometheus monitoring integration"))
    {
        _client = new PrometheusHttpClient(options.CurrentValue.Endpoint);
        _options = options;

        logger.LogInformation("Prometheus integration initialized.");
    }

    IEnumerable<IIntegrationMetric> IMetricProvider.Metrics =>
        _options.CurrentValue.Queries.Select(query =>
            new IntegrationMetric(
                query.Name,
                async _ =>
                {
                    var result = await QueryAsync(query.PromQL);
                    return result.Data.Result.FirstOrDefault()?.Value[1];
                }));

    public override async Task<IntegrationHealthStatus> PerformHealthCheck()
    {
        try
        {
            if (!await _client.IsHealthyAsync())
                return IntegrationHealthStatus.Unhealthy;

            var runtime = await _client.GetRuntimeInfoAsync();

            return runtime.Status == "success"
                ? IntegrationHealthStatus.Healthy
                : IntegrationHealthStatus.Unhealthy;
        }
        catch
        {
            return IntegrationHealthStatus.Unhealthy;
        }
    }

    public Task<bool> IsHealthyAsync()
        => _client.IsHealthyAsync();

    public Task<PrometheusRuntimeInfoResponse> GetRuntimeInfoAsync()
        => _client.GetRuntimeInfoAsync();

    public Task<PrometheusTargetsResponse> GetTargetsAsync()
        => _client.GetTargetsAsync();

    public Task<PrometheusAlertsResponse> GetAlertsAsync()
        => _client.GetAlertsAsync();

    public Task<PrometheusQueryResponse> QueryAsync(string promQl)
        => _client.QueryAsync(promQl);

    public Task<PrometheusRangeQueryResponse> QueryRangeAsync(
        string promQl,
        DateTimeOffset start,
        DateTimeOffset end,
        TimeSpan step)
        => _client.QueryRangeAsync(promQl, start, end, step);
}