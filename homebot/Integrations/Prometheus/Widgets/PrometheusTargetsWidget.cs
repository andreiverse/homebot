using HomeBot.Display;
using HomeBot.Discord;

namespace HomeBot.Integrations.Prometheus.Widgets;

public class PrometheusTargetsWidget(PrometheusIntegration prometheus) : IWidget
{
    public string Id => "prometheus.targets";
    public string Name => "Scrape Targets";
    public string? Description => "Shows scrape target status";

    public async Task<Card> RenderAsync(CancellationToken ct = default)
    {
        var targets = await prometheus.GetTargetsAsync(ct);

        var active = targets.Data.ActiveTargets.Count;
        var healthy = targets.Data.ActiveTargets.Count(x => x.Health == "up");

        return new Card()
            .WithHeading("🎯 Prometheus Targets")
            .WithAccent(BrandColors.Prometheus)
            .AddKeyValueBlock(kv => kv
                .Add("Healthy", $"🟢 {healthy}")
                .Add("Unhealthy", $"🔴 {active - healthy}")
                .Add("Total", active)
            );
    }
}
