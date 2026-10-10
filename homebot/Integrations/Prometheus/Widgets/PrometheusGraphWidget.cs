using HomeBot.Display;
using HomeBot.Discord;
using Microsoft.Extensions.Options;
using System.Globalization;
using ScottPlot;

namespace HomeBot.Integrations.Prometheus.Widgets;

public record PrometheusGraphWidgetOptions(string Query = "up", int Hours = 1);

public class PrometheusGraphWidget(
    PrometheusIntegration prometheus,
    IOptions<PrometheusOptions> options) : IWidget
{
    private readonly PrometheusOptions _options = options.Value;

    public bool McpEnabled => false;

    public string Id => "prometheus.graph";
    public string Name => "Prometheus Graph";
    public string? Description => "Renders a Prometheus metric as a time-series graph";

    public Type? OptionsType => typeof(PrometheusGraphWidgetOptions);

    public Task<Card> RenderAsync(object? options = null, CancellationToken ct = default)
    {
        var opt = options as PrometheusGraphWidgetOptions ?? new PrometheusGraphWidgetOptions();
        return RenderAsync(opt.Query, opt.Hours, ct);
    }

    private static async Task<MemoryStream> RenderGraphAsync(GraphBlock graph, CancellationToken ct)
    {
        var plot = new Plot();

        foreach (var series in graph.Series)
        {
            var xs = graph.IsDateTimeAxis
                ? series.Xs.Select(x => DateTimeOffset.FromUnixTimeMilliseconds((long)x).UtcDateTime.ToOADate()).ToArray()
                : series.Xs;

            var scatter = plot.Add.Scatter(xs, series.Ys);
            scatter.LineWidth = 5;

            if (series.Name is not null)
                scatter.LegendText = series.Name;
        }

        plot.Title(graph.Title);
        plot.XLabel(graph.XLabel);
        plot.YLabel(graph.YLabel);

        if (graph.IsDateTimeAxis)
            plot.Axes.DateTimeTicksBottom();

        plot.Axes.AutoScale();

        var file = Path.GetTempFileName() + ".png";
        plot.SavePng(file, 800, 400);

        var bytes = await File.ReadAllBytesAsync(file, ct);
        File.Delete(file);

        return new MemoryStream(bytes);
    }

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
                .Select(v => v[0].GetDouble() * 1000) // Keep as Unix milliseconds
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
            // .AddContent(graph)
            .WithImageStream(await RenderGraphAsync(graph, ct));
    }
}
