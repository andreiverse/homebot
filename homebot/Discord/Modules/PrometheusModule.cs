using HomeBot.Integrations.Prometheus;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using ScottPlot;
using System.Globalization;
namespace HomeBot.Discord.Modules;

[SlashCommand("prometheus", "Prometheus monitoring commands")]
public class PrometheusModule(PrometheusIntegration prometheus)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("info", "Show Prometheus runtime information")]
    public async Task<InteractionMessageProperties> Info()
    {
        var runtime = await prometheus.GetRuntimeInfoAsync();

        var embed = new EmbedProperties()
            .WithTitle("📈 Prometheus")
            .WithColor(new NetCord.Color(0xE6522C))
            .AddFields(
                new EmbedFieldProperties()
                    .WithName("Started")
                    .WithValue(runtime.Data.StartTime.ToString("u"))
                    .WithInline(true),

                new EmbedFieldProperties()
                    .WithName("Time Series")
                    .WithValue(runtime.Data.TimeSeriesCount.ToString("N0"))
                    .WithInline(true),

                new EmbedFieldProperties()
                    .WithName("Go Routines")
                    .WithValue(runtime.Data.GoroutineCount.ToString())
                    .WithInline(true),

                new EmbedFieldProperties()
                    .WithName("Storage Retention")
                    .WithValue(runtime.Data.StorageRetention)
                    .WithInline(true),

                new EmbedFieldProperties()
                    .WithName("Config Reload")
                    .WithValue(runtime.Data.ReloadConfigSuccess ? "🟢 Success" : "🔴 Failed")
                    .WithInline(true)
            );

        return new InteractionMessageProperties
        {
            Embeds = new[] { embed }
        };
    }

    [SubSlashCommand("targets", "Show scrape target status")]
    public async Task<InteractionMessageProperties> Targets()
    {
        var targets = await prometheus.GetTargetsAsync();

        var active = targets.Data.ActiveTargets.Count;
        var healthy = targets.Data.ActiveTargets.Count(x => x.Health == "up");

        var embed = new EmbedProperties()
            .WithTitle("🎯 Prometheus Targets")
            .WithColor(new NetCord.Color(0xE6522C))
            .AddFields(
                new EmbedFieldProperties()
                    .WithName("Healthy")
                    .WithValue($"🟢 {healthy}")
                    .WithInline(true),

                new EmbedFieldProperties()
                    .WithName("Unhealthy")
                    .WithValue($"🔴 {active - healthy}")
                    .WithInline(true),

                new EmbedFieldProperties()
                    .WithName("Total")
                    .WithValue(active.ToString())
                    .WithInline(true)
            );

        return new InteractionMessageProperties
        {
            Embeds = new[] { embed }
        };
    }

    [SubSlashCommand("alerts", "Show active alerts")]
    public async Task<InteractionMessageProperties> Alerts()
    {
        var alerts = await prometheus.GetAlertsAsync();

        var active = alerts.Data.Alerts.Count;

        var embed = new EmbedProperties()
            .WithTitle("🚨 Prometheus Alerts")
            .WithColor(active == 0 ? new NetCord.Color(0x43B581) : new NetCord.Color(0xF04747))
            .WithDescription(
                active == 0
                    ? "No active alerts."
                    : $"{active} alert(s) currently firing.");

        if (active > 0)
        {
            foreach (var alert in alerts.Data.Alerts.Take(10))
            {
                embed.AddFields(new EmbedFieldProperties()
                    .WithName(alert.Labels["alertname"])
                    .WithValue(alert.State)
                    .WithInline(true));
            }
        }

        return new InteractionMessageProperties
        {
            Embeds = new[] { embed }
        };
    }

    [SubSlashCommand("query", "Execute a PromQL query")]
    public async Task<InteractionMessageProperties> Query(
        [SlashCommandParameter(Description = "PromQL expression")]
        string query)
    {
        var result = await prometheus.QueryAsync(query);

        var embed = new EmbedProperties()
            .WithTitle("🔎 PromQL Query")
            .WithColor(new NetCord.Color(0xE6522C))
            .WithDescription($"```promql\n{query}\n```");

        if (result.Data.Result.Count == 0)
        {
            embed.AddFields(new EmbedFieldProperties()
                .WithName("Result")
                .WithValue("No data returned."));
        }
        else
        {
            foreach (var metric in result.Data.Result.Take(10))
            {
                var name = metric.Metric.TryGetValue("__name__", out var n)
                    ? n
                    : "metric";

                var value = metric.Value.Count >= 2
                    ? metric.Value[1]?.ToString() ?? "N/A"
                    : "N/A";

                embed.AddFields(new EmbedFieldProperties()
                    .WithName(name)
                    .WithValue(value)
                    .WithInline(true));
            }
        }

        return new InteractionMessageProperties
        {
            Embeds = new[] { embed }
        };
    }

    [SubSlashCommand("graph", "Render a Prometheus graph")]
    public async Task<InteractionMessageProperties> Graph(
    [SlashCommandParameter(Description = "PromQL query")]
    string query,

    [SlashCommandParameter(Description = "Hours back")]
    int hours = 1)
    {
        var end = DateTimeOffset.UtcNow;
        var start = end.AddHours(-hours);

        var response = await prometheus.QueryRangeAsync(
            query,
            start,
            end,
            TimeSpan.FromMinutes(1));

        if (response.Data.Result.Count == 0)
        {
            return new InteractionMessageProperties
            {
                Content = "No data returned."
            };
        }

        var values = response.Data.Result[0].Values;

        if (values.Count == 0)
        {
            return new InteractionMessageProperties
            {
                Content = "No data returned."
            };
        }

        double[] xs = values
    .Select(v =>
        DateTimeOffset
            .FromUnixTimeMilliseconds((long)(v[0].GetDouble() * 1000))
            .UtcDateTime
            .ToOADate())
    .ToArray();

        double[] ys = values
            .Select(v => double.Parse(v[1].GetString()!, CultureInfo.InvariantCulture))
            .ToArray();


        var plot = new Plot();

        var scat = plot.Add.Scatter(xs, ys);
        scat.LineWidth = 5;

        plot.Title(query);
        plot.XLabel("Time");
        plot.YLabel("Value");

        plot.Axes.DateTimeTicksBottom();
        plot.Axes.AutoScale();

        var file = Path.GetTempFileName() + ".png";

        plot.SavePng(file, 800, 400);

        var ms = new MemoryStream(await File.ReadAllBytesAsync(file));

        File.Delete(file);

        return new InteractionMessageProperties
        {
            Embeds =
            [
                new EmbedProperties()
                .WithTitle("📈 Prometheus Graph")
                .WithDescription($"```promql\n{query}\n```")
                .WithImage(new EmbedImageProperties("attachment://graph.png"))
            ],
            Attachments =
            [
                new AttachmentProperties("graph.png", ms)
            ]
        };
    }
}