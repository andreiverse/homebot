using HomeBot.Display;
using HomeBot.Discord;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace HomeBot.Integrations.Prometheus.Widgets;

public class PrometheusGraphWidget(
    PrometheusIntegration prometheus,
    IOptions<PrometheusOptions> options) : IWidget
{
    private readonly PrometheusOptions _options = options.Value;

    public string Id => "prometheus.graph";
    public string Name => "Prometheus Graph";
    public string? Description => "Renders a Prometheus metric as a time-series graph";

    public Task<Card> RenderAsync(CancellationToken ct = default)
        => RenderAsync("up", 1, ct);

    public async Task<Card> RenderAsync(
        string query,
        int hours,
        CancellationToken ct = default)
    {
        if (query.StartsWith("{PREDEFINED}:"))
        {
            query = _options.Queries[int.Parse(query.Split(":")[1])].PromQL;
        }
        else if (_options.OnlyAllowDefinedQueries == true)
        {
            return new Card()
                .WithHeading("Query is not defined")
                .WithAccent(BrandColors.Prometheus);
        }

        var end = DateTimeOffset.UtcNow;
        var start = end.AddHours(-hours);

        var response = await prometheus.QueryRangeAsync(
            query, start, end, TimeSpan.FromMinutes(1), ct);

        if (response.Data.Result.Count == 0 || response.Data.Result.All(r => r.Values.Count == 0))
        {
            return new Card()
                .WithHeading("📈 Prometheus Graph")
                .WithSummary("No data returned.")
                .WithAccent(BrandColors.Prometheus);
        }

        var graph = new GraphBlock
        {
            Title = query,
            XLabel = "Time",
            YLabel = "Value",
            IsDateTimeAxis = true,
            Series = []
        };

        foreach (var metric in response.Data.Result)
        {
            if (metric.Values.Count == 0) continue;

            double[] xs = metric.Values
                .Select(v => DateTimeOffset
                    .FromUnixTimeMilliseconds((long)(v[0].GetDouble() * 1000))
                    .UtcDateTime
                    .ToOADate())
                .ToArray();

            double[] ys = metric.Values
                .Select(v => double.Parse(v[1].GetString()!, CultureInfo.InvariantCulture))
                .ToArray();

            var seriesName = metric.Metric.TryGetValue("instance", out var instance) ? instance :
                             metric.Metric.TryGetValue("job", out var job) ? job :
                             metric.Metric.TryGetValue("__name__", out var name) ? name : null;

            graph.Series.Add(new GraphSeries { Name = seriesName, Xs = xs, Ys = ys });
        }

        return new Card()
            .WithHeading("📈 Prometheus Graph")
            .WithAccent(BrandColors.Prometheus)
            .AddCodeBlock(query, "promql")
            .AddContent(graph);
    }
}
