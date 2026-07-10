using HomeBot.Display;
using HomeBot.Integrations.Prometheus;
using Microsoft.Extensions.Options;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using ScottPlot;
using System.Globalization;

namespace HomeBot.Discord.Modules;

[SlashCommand("prometheus", "Prometheus monitoring commands")]
public class PrometheusModule(PrometheusIntegration prometheus, IOptions<PrometheusOptions> options)
    : ApplicationCommandModule<ApplicationCommandContext>
{

    [SubSlashCommand("info", "Show Prometheus runtime information")]
    public async Task<InteractionMessageProperties> Info()
    {
        var runtime = await prometheus.GetRuntimeInfoAsync();

        var card = new Card
        {
            Heading = "📈 Prometheus",
            Accent = BrandColors.Prometheus,
            Content =
            [
                new KeyValueBlock
                {
                    Items =
                    [
                        new() { Key = "Started", Value = runtime.Data.StartTime.ToString("u") },
                        new() { Key = "Time Series", Value = runtime.Data.TimeSeriesCount.ToString("N0") },
                        new() { Key = "Go Routines", Value = runtime.Data.GoroutineCount.ToString() },
                        new() { Key = "Storage Retention", Value = runtime.Data.StorageRetention },
                        new() { Key = "Config Reload", Value = runtime.Data.ReloadConfigSuccess ? "🟢 Success" : "🔴 Failed" }
                    ]
                }
            ]
        };

        return card.ToInteractionMessage();
    }

    [SubSlashCommand("targets", "Show scrape target status")]
    public async Task<InteractionMessageProperties> Targets()
    {
        var targets = await prometheus.GetTargetsAsync();

        var active = targets.Data.ActiveTargets.Count;
        var healthy = targets.Data.ActiveTargets.Count(x => x.Health == "up");

        var card = new Card
        {
            Heading = "🎯 Prometheus Targets",
            Accent = BrandColors.Prometheus,
            Content =
            [
                new KeyValueBlock
                {
                    Items =
                    [
                        new() { Key = "Healthy", Value = $"🟢 {healthy}" },
                        new() { Key = "Unhealthy", Value = $"🔴 {active - healthy}" },
                        new() { Key = "Total", Value = active.ToString() }
                    ]
                }
            ]
        };

        return card.ToInteractionMessage();
    }

    [SubSlashCommand("alerts", "Show active alerts")]
    public async Task<InteractionMessageProperties> Alerts()
    {
        var alerts = await prometheus.GetAlertsAsync();

        var active = alerts.Data.Alerts.Count;

        var card = new Card
        {
            Heading = "🚨 Prometheus Alerts",
            Accent = active == 0 ? BrandColors.AlertsClear : BrandColors.AlertsFiring,
            Summary = active == 0
                ? "No active alerts."
                : $"{active} alert(s) currently firing."
        };

        if (active > 0)
        {
            card.Content.Add(new KeyValueBlock
            {
                Items = alerts.Data.Alerts
                    .Take(10)
                    .Select(alert => new KeyValueItem
                    {
                        Key = alert.Labels["alertname"],
                        Value = alert.State
                    })
                    .ToList()
            });
        }

        return card.ToInteractionMessage();
    }

    [SubSlashCommand("query", "Execute a PromQL query")]
    public async Task<InteractionMessageProperties> Query(
        [SlashCommandParameter(Description = "PromQL expression")]
        string query)
    {
        var result = await prometheus.QueryAsync(query);

        var card = new Card
        {
            Heading = "🔎 PromQL Query",
            Accent = BrandColors.Prometheus,
            Content =
            [
                new CodeBlock
                {
                    Language = "promql",
                    Code = query
                }
            ]
        };

        if (result.Data.Result.Count == 0)
        {
            card.Content.Add(new KeyValueBlock
            {
                Items =
                [
                    new()
                    {
                        Key = "Result",
                        Value = "No data returned."
                    }
                ]
            });
        }
        else
        {
            card.Content.Add(new KeyValueBlock
            {
                Items = result.Data.Result
                    .Take(10)
                    .Select(metric => new KeyValueItem
                    {
                        Key = metric.Metric.TryGetValue("__name__", out var name)
                            ? name
                            : "metric",
                        Value = metric.Value.Count >= 2
                            ? metric.Value[1]?.ToString() ?? "N/A"
                            : "N/A"
                    })
                    .ToList()
            });
        }

        return card.ToInteractionMessage();
    }

    [SubSlashCommand("graph", "Render a Prometheus graph")]
    public async Task<InteractionMessageProperties> Graph(
        [SlashCommandParameter(Description = "PromQL query", AutocompleteProviderType=typeof(PrometheusQueryAutocomplete))]
        string query,

        [SlashCommandParameter(Description = "Hours back")]
        int hours = 1)
    {
        if (query.StartsWith("{PREDEFINED}:"))
        {
            query = options.Value.Queries[Int32.Parse(query.Split(":")[1])].PromQL;
        }
        else if (options.Value.OnlyAllowDefinedQueries == true)
        {

            var c = new Card
            {
                Heading = "Query is not defined",
            };
            return c.ToInteractionMessage();

        }

        var end = DateTimeOffset.UtcNow;
        var start = end.AddHours(-hours);

        var response = await prometheus.QueryRangeAsync(
            query,
            start,
            end,
            TimeSpan.FromMinutes(1));

        if (response.Data.Result.Count == 0)
            return new() { Content = "No data returned." };

        var values = response.Data.Result[0].Values;

        if (values.Count == 0)
            return new() { Content = "No data returned." };

        double[] xs = values
            .Select(v => DateTimeOffset
                .FromUnixTimeMilliseconds((long)(v[0].GetDouble() * 1000))
                .UtcDateTime
                .ToOADate())
            .ToArray();

        double[] ys = values
            .Select(v => double.Parse(v[1].GetString()!, CultureInfo.InvariantCulture))
            .ToArray();

        var plot = new Plot();

        var scatter = plot.Add.Scatter(xs, ys);
        scatter.LineWidth = 5;

        plot.Title(query);
        plot.XLabel("Time");
        plot.YLabel("Value");
        plot.Axes.DateTimeTicksBottom();
        plot.Axes.AutoScale();

        var file = Path.GetTempFileName() + ".png";

        plot.SavePng(file, 800, 400);

        var ms = new MemoryStream(await File.ReadAllBytesAsync(file));

        File.Delete(file);

        var card = new Card
        {
            Heading = "📈 Prometheus Graph",
            Content =
            [
                new CodeBlock
                {
                    Language = "promql",
                    Code = query
                }
            ]
        };

        var embed = card.ToDiscordEmbed()
            .WithImage(new EmbedImageProperties("attachment://graph.png"));

        return new()
        {
            Embeds = [embed],
            Attachments =
            [
                new AttachmentProperties("graph.png", ms)
            ]
        };
    }
}