using HomeBot.Display;
using HomeBot.Integrations;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace HomeBot.Discord.Modules;

[SlashCommand("metrics", "View integration metrics")]
public class MetricModule(IntegrationMetricManager metrics)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("list", "Show all metrics")]
    public async Task<InteractionMessageProperties> List()
    {
        var snapshot = metrics.GetSnapshot();

        var card = new Card
        {
            Heading = "📊 Integration Metrics",
            Accent = BrandColors.Metrics
        };

        card.Content.Add(new KeyValueBlock
        {
            Items = snapshot
                .OrderBy(x => x.Metric.Name)
                .Select(x => new KeyValueItem
                {
                    Key = $"{x.Metric.Name} (`{x.Metric.Id}`)",
                    Value = x.Value?.ToString() ?? "N/A"
                })
                .ToList()
        });

        return card.ToInteractionMessage();
    }
}