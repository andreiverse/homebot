using HomeBot.Display;
using HomeBot.Discord;

namespace HomeBot.Integrations.Prometheus.Widgets;

public class PrometheusAlertsWidget(PrometheusIntegration prometheus) : IWidget
{
    public string Id => "prometheus.alerts";
    public string Name => "Active Alerts";
    public string? Description => "Shows active Prometheus alerts";

    public async Task<Card> RenderAsync(CancellationToken ct = default)
    {
        var alerts = await prometheus.GetAlertsAsync(ct);
        var active = alerts.Data.Alerts.Count;

        var card = new Card()
            .WithHeading("🚨 Prometheus Alerts")
            .WithAccent(active == 0 ? BrandColors.AlertsClear : BrandColors.AlertsFiring)
            .WithSummary(active == 0 ? "No active alerts." : $"{active} alert(s) currently firing.");

        if (active > 0)
        {
            card.AddKeyValueBlock(kv =>
            {
                foreach (var alert in alerts.Data.Alerts.Take(10))
                {
                    kv.Add(
                        alert.Labels.TryGetValue("alertname", out var alertname) ? alertname : "unknown",
                        alert.State);
                }
            });
        }

        return card;
    }
}
