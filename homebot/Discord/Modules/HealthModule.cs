using HomeBot.Integrations;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace HomeBot.Discord.Modules;

public class HealthModule(IntegrationManager integrationManager)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("health", "Check integration health")]
    public async Task<InteractionMessageProperties> Health()
    {
        var healthz = await integrationManager.PerformHealthChecks();

        var color = healthz.Values.Contains(IntegrationHealthStatus.Unhealthy)
            ? new Color(0xED4245)   // Red
            : healthz.Values.Contains(IntegrationHealthStatus.Unknown)
                ? new Color(0xFEE75C) // Yellow
                : new Color(0x57F287); // Green

        var embed = new EmbedProperties()
            .WithTitle("Integration Health")
            .WithColor(new Color(0x57F287)); // Green

        embed.AddFields(
            healthz.Select(x => new EmbedFieldProperties()
                .WithName(x.Key.Name)
                .WithValue(x.Value switch
                    {
                        IntegrationHealthStatus.Healthy => "🟢 Healthy",
                        IntegrationHealthStatus.Unknown => "🟡 Unknown",
                        IntegrationHealthStatus.Unhealthy => "🔴 Unhealthy",
                        _ => "⚪ Unknown"
                    })
                .WithInline(true)
            )
        );

        return new InteractionMessageProperties
        {
            Embeds = new[] { embed }
        };
    }
}