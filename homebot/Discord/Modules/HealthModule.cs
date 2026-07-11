using HomeBot.Display;
using HomeBot.Integrations;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace HomeBot.Discord.Modules;

public class HealthModule(IntegrationManager integrationManager)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("health", "Check integration health")]
    public async Task Health()
    {
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage());

        var healthz = await integrationManager.PerformHealthChecks();

        var accent = healthz.Values.Contains(IntegrationHealthStatus.Unhealthy)
            ? BrandColors.HealthUnhealthy
            : healthz.Values.Contains(IntegrationHealthStatus.Unknown)
                ? BrandColors.HealthUnknown
                : BrandColors.HealthHealthy;

        var card = new Card
        {
            Heading = "Integration Health",
            Accent = accent,
            Content =
            [
                new KeyValueBlock
                {
                    Items = healthz.Select(x => new KeyValueItem
                    {
                        Key = x.Key.Name,
                        Value = x.Value switch
                        {
                            IntegrationHealthStatus.Healthy => "🟢 Healthy",
                            IntegrationHealthStatus.Unknown => "🟡 Unknown",
                            IntegrationHealthStatus.Unhealthy => "🔴 Unhealthy",
                            _ => "⚪ Unknown"
                        }
                    }).ToList()
                }
            ]
        };

        await Context.Interaction.ModifyResponseAsync(message =>
        {
            message.Embeds = [card.ToDiscordEmbed()];
        });
    }
}