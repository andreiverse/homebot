using HomeBot.Display;
using HomeBot.Discord;

namespace HomeBot.Integrations.Prometheus.Widgets;

public class PrometheusQueryWidget(PrometheusIntegration prometheus) : IWidget
{
    public string Id => "prometheus.query";
    public string Name => "PromQL Query";
    public string? Description => "Executes a PromQL query and shows the result";

    public Task<Card> RenderAsync(CancellationToken ct = default)
        => RenderAsync("up", ct);

    public async Task<Card> RenderAsync(string query, CancellationToken ct = default)
    {
        var result = await prometheus.QueryAsync(query, ct);

        var card = new Card()
            .WithHeading("🔎 PromQL Query")
            .WithAccent(BrandColors.Prometheus)
            .AddCodeBlock(query, "promql");

        if (result.Data.Result.Count == 0)
        {
            card.AddKeyValueBlock(kv => kv.Add("Result", "No data returned."));
        }
        else
        {
            card.AddKeyValueBlock(kv =>
            {
                foreach (var metric in result.Data.Result.Take(10))
                {
                    var key = metric.Metric.TryGetValue("__name__", out var name) ? name : "metric";
                    var value = metric.Value.Count >= 2 ? metric.Value[1]?.ToString() ?? "N/A" : "N/A";
                    kv.Add(key, value);
                }
            });
        }

        return card;
    }
}
