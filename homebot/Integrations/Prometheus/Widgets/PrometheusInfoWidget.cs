using HomeBot.Display;
using HomeBot.Discord;

namespace HomeBot.Integrations.Prometheus.Widgets;

public class PrometheusInfoWidget(PrometheusIntegration prometheus) : IWidget
{
    public string Id => "prometheus.info";
    public string Name => "Prometheus Info";
    public string? Description => "Shows Prometheus runtime information";

    public async Task<Card> RenderAsync(CancellationToken ct = default)
    {
        var runtime = await prometheus.GetRuntimeInfoAsync(ct);

        return new Card()
            .WithHeading("📈 Prometheus")
            .WithAccent(BrandColors.Prometheus)
            .AddKeyValueBlock(kv => kv
                .Add("Started", runtime.Data.StartTime.ToString("u"))
                .Add("Time Series", runtime.Data.TimeSeriesCount.ToString("N0"))
                .Add("Go Routines", runtime.Data.GoroutineCount)
                .Add("Storage Retention", runtime.Data.StorageRetention)
                .Add("Config Reload", runtime.Data.ReloadConfigSuccess ? "🟢 Success" : "🔴 Failed")
            );
    }
}
