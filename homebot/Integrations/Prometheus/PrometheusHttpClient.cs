namespace HomeBot.Integrations.Prometheus;

public sealed class PrometheusHttpClient
{
    private readonly HttpClient _http;

    public PrometheusHttpClient(PrometheusOptions prometheusOptions)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(prometheusOptions.Endpoint.TrimEnd('/'))
        };
    }

    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        using var response = await _http.GetAsync("/-/healthy", ct);
        return response.IsSuccessStatusCode;
    }

    public Task<PrometheusRuntimeInfoResponse> GetRuntimeInfoAsync(
        CancellationToken ct = default)
        => _http.GetFromJsonRequiredAsync<PrometheusRuntimeInfoResponse>(
            "/api/v1/status/runtimeinfo", ct);

    public Task<PrometheusTargetsResponse> GetTargetsAsync(
        CancellationToken ct = default)
        => _http.GetFromJsonRequiredAsync<PrometheusTargetsResponse>(
            "/api/v1/targets", ct);

    public Task<PrometheusAlertsResponse> GetAlertsAsync(
        CancellationToken ct = default)
        => _http.GetFromJsonRequiredAsync<PrometheusAlertsResponse>(
            "/api/v1/alerts", ct);

    public Task<PrometheusQueryResponse> QueryAsync(
        string promQl,
        CancellationToken ct = default)
    {
        var url = $"/api/v1/query?query={Uri.EscapeDataString(promQl)}";

        return _http.GetFromJsonRequiredAsync<PrometheusQueryResponse>(url, ct);
    }

    public Task<PrometheusRangeQueryResponse> QueryRangeAsync(
        string promQl,
        DateTimeOffset start,
        DateTimeOffset end,
        TimeSpan step,
        CancellationToken ct = default)
    {
        var url =
            $"/api/v1/query_range" +
            $"?query={Uri.EscapeDataString(promQl)}" +
            $"&start={start.ToUnixTimeSeconds()}" +
            $"&end={end.ToUnixTimeSeconds()}" +
            $"&step={(int)step.TotalSeconds}";

        return _http.GetFromJsonRequiredAsync<PrometheusRangeQueryResponse>(url, ct);
    }
}
