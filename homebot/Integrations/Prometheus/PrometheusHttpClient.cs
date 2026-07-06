using System.Net.Http.Json;

namespace HomeBot.Integrations.Prometheus;

public sealed class PrometheusHttpClient
{
    private readonly HttpClient _http;

    public PrometheusHttpClient(string endpoint)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(endpoint.TrimEnd('/'))
        };
    }

    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        using var response = await _http.GetAsync("/-/healthy", ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<PrometheusRuntimeInfoResponse> GetRuntimeInfoAsync(
        CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<PrometheusRuntimeInfoResponse>(
                   "/api/v1/status/runtimeinfo",
                   ct)
               ?? throw new InvalidOperationException("No response.");
    }

    public async Task<PrometheusTargetsResponse> GetTargetsAsync(
        CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<PrometheusTargetsResponse>(
                   "/api/v1/targets",
                   ct)
               ?? throw new InvalidOperationException("No response.");
    }

    public async Task<PrometheusAlertsResponse> GetAlertsAsync(
        CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<PrometheusAlertsResponse>(
                   "/api/v1/alerts",
                   ct)
               ?? throw new InvalidOperationException("No response.");
    }

    public async Task<PrometheusQueryResponse> QueryAsync(
        string promQl,
        CancellationToken ct = default)
    {
        var url = $"/api/v1/query?query={Uri.EscapeDataString(promQl)}";

        return await _http.GetFromJsonAsync<PrometheusQueryResponse>(url, ct)
               ?? throw new InvalidOperationException("No response.");
    }

    public async Task<PrometheusRangeQueryResponse> QueryRangeAsync(
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

        return await _http.GetFromJsonAsync<PrometheusRangeQueryResponse>(url, ct)
               ?? throw new InvalidOperationException("No response.");
    }
}