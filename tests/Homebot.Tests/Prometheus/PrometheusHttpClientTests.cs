using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using HomeBot.Integrations.Prometheus;

namespace Homebot.Tests.Prometheus;

/// <summary>
/// Shared fixture that starts a single Prometheus container for the entire test class.
/// Using IClassFixture keeps startup cost low and gives Prometheus time to complete
/// its first scrape before any test assertion runs.
/// </summary>
public sealed class PrometheusContainerFixture : IAsyncLifetime
{
    private const int PrometheusPort = 9090;

    private static readonly string ConfigPath =
        Path.Combine(AppContext.BaseDirectory, "Prometheus", "prometheus.yml");

    private readonly IContainer _container;

    public PrometheusContainerFixture()
    {
        _container = new ContainerBuilder("prom/prometheus:latest")
            .WithPortBinding(PrometheusPort, assignRandomHostPort: true)
            .WithBindMount(ConfigPath, "/etc/prometheus/prometheus.yml")
            // Primary wait: Prometheus HTTP endpoint is up.
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r
                        .ForPort(PrometheusPort)
                        .ForPath("/-/healthy")))
            .Build();
    }

    public string Endpoint { get; private set; } = "";

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var port = _container.GetMappedPublicPort(PrometheusPort);
        Endpoint = $"http://localhost:{port}";

        // Secondary wait: poll until a range query for 'up' returns data.
        // This guarantees step-aligned evaluation timestamps have at least one
        // sample in their lookback window before any test assertion runs.
        using var http = new HttpClient { BaseAddress = new Uri(Endpoint) };

        for (var i = 0; i < 30; i++)
        {
            try
            {
                var end = DateTimeOffset.UtcNow;
                var start = end.AddSeconds(-30);
                var url = $"/api/v1/query_range?query=up" +
                          $"&start={start.ToUnixTimeSeconds()}" +
                          $"&end={end.ToUnixTimeSeconds()}" +
                          $"&step=1";

                var resp = await http.GetStringAsync(url);
                if (resp.Contains("\"result\":[{"))
                    return; // range data is ready
            }
            catch
            {
                // not ready yet
            }

            await Task.Delay(500);
        }

        throw new TimeoutException("Prometheus did not return range query data within 15 seconds.");
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

/// <summary>
/// Integration tests for <see cref="PrometheusHttpClient"/> using a shared Prometheus
/// container. The container is started once, and the scrape interval is set to 1s so
/// data is available within seconds of startup.
/// </summary>
public sealed class PrometheusHttpClientTests(PrometheusContainerFixture fixture)
    : IClassFixture<PrometheusContainerFixture>
{
    private PrometheusHttpClient CreateClient() =>
        new(new PrometheusOptions { Endpoint = fixture.Endpoint });

    // -------------------------------------------------------------------------
    // IsHealthyAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task IsHealthyAsync_WhenContainerIsRunning_ReturnsTrue()
    {
        var result = await CreateClient().IsHealthyAsync();

        result.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // GetRuntimeInfoAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetRuntimeInfoAsync_ReturnsSuccessStatus()
    {
        var response = await CreateClient().GetRuntimeInfoAsync();

        response.Status.Should().Be("success");
    }

    [Fact]
    public async Task GetRuntimeInfoAsync_ReturnsNonEmptyStorageRetention()
    {
        var response = await CreateClient().GetRuntimeInfoAsync();

        response.Data.StorageRetention.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetRuntimeInfoAsync_ReturnsPositiveGoroutineCount()
    {
        var response = await CreateClient().GetRuntimeInfoAsync();

        response.Data.GoroutineCount.Should().BePositive();
    }

    // -------------------------------------------------------------------------
    // GetTargetsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetTargetsAsync_ReturnsSuccessStatus()
    {
        var response = await CreateClient().GetTargetsAsync();

        response.Status.Should().Be("success");
    }

    [Fact]
    public async Task GetTargetsAsync_ReturnsAtLeastOneActiveTarget()
    {
        var response = await CreateClient().GetTargetsAsync();

        response.Data.ActiveTargets.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetTargetsAsync_ActiveTargetsHaveHealthProperty()
    {
        var response = await CreateClient().GetTargetsAsync();

        response.Data.ActiveTargets
            .Should().AllSatisfy(t => t.Health.Should().NotBeNullOrWhiteSpace());
    }

    // -------------------------------------------------------------------------
    // GetAlertsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAlertsAsync_ReturnsSuccessStatus()
    {
        var response = await CreateClient().GetAlertsAsync();

        response.Status.Should().Be("success");
    }

    [Fact]
    public async Task GetAlertsAsync_ReturnsAlertsCollection()
    {
        var response = await CreateClient().GetAlertsAsync();

        // A freshly-started Prometheus has no firing alerts; the list must be non-null.
        response.Data.Alerts.Should().NotBeNull();
    }

    // -------------------------------------------------------------------------
    // QueryAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task QueryAsync_WithValidQuery_ReturnsSuccessStatus()
    {
        var response = await CreateClient().QueryAsync("up");

        response.Status.Should().Be("success");
    }

    [Fact]
    public async Task QueryAsync_WithUpQuery_ReturnsVectorResultType()
    {
        var response = await CreateClient().QueryAsync("up");

        response.Data.ResultType.Should().Be("vector");
    }

    [Fact]
    public async Task QueryAsync_WithUpQuery_ReturnsAtLeastOneResult()
    {
        var response = await CreateClient().QueryAsync("up");

        response.Data.Result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task QueryAsync_WithUpQuery_ResultsHaveMetricAndValuePairs()
    {
        var response = await CreateClient().QueryAsync("up");

        response.Data.Result.Should().AllSatisfy(r =>
        {
            r.Metric.Should().NotBeNull();
            r.Value.Should().HaveCountGreaterThanOrEqualTo(2,
                "each instant vector result must have a [timestamp, value] pair");
        });
    }

    // -------------------------------------------------------------------------
    // QueryRangeAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task QueryRangeAsync_WithValidQuery_ReturnsSuccessStatus()
    {
        var end = DateTimeOffset.UtcNow;
        var start = end.AddSeconds(-30);

        var response = await CreateClient().QueryRangeAsync("up", start, end, TimeSpan.FromSeconds(1));

        response.Status.Should().Be("success");
    }

    [Fact]
    public async Task QueryRangeAsync_WithValidQuery_ReturnsMatrixResultType()
    {
        var end = DateTimeOffset.UtcNow;
        var start = end.AddSeconds(-30);

        var response = await CreateClient().QueryRangeAsync("up", start, end, TimeSpan.FromSeconds(1));

        response.Data.ResultType.Should().Be("matrix");
    }

    [Fact]
    public async Task QueryRangeAsync_WithValidQuery_ReturnsResults()
    {
        var end = DateTimeOffset.UtcNow;
        var start = end.AddSeconds(-30);

        var response = await CreateClient().QueryRangeAsync("up", start, end, TimeSpan.FromSeconds(1));

        response.Data.Result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task QueryRangeAsync_ResultsContainTimestampValuePairs()
    {
        var end = DateTimeOffset.UtcNow;
        var start = end.AddSeconds(-30);

        var response = await CreateClient().QueryRangeAsync("up", start, end, TimeSpan.FromSeconds(1));

        response.Data.Result.Should().AllSatisfy(r =>
        {
            r.Values.Should().NotBeEmpty("each range result must contain at least one data point");
            r.Values.Should().AllSatisfy(pair =>
                pair.Should().HaveCount(2, "each data point is a [timestamp, value] pair"));
        });
    }
}
