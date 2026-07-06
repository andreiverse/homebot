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
        // Immediately acknowledge the interaction
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage()
        );

        var healthz = await integrationManager.PerformHealthChecks();

        var color = healthz.Values.Contains(IntegrationHealthStatus.Unhealthy)
            ? new Color(0xED4245)
            : healthz.Values.Contains(IntegrationHealthStatus.Unknown)
                ? new Color(0xFEE75C)
                : new Color(0x57F287);

        var embed = new EmbedProperties()
            .WithTitle("Integration Health")
            .WithColor(color);

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
                .WithInline(true))
        );

        await Context.Interaction.ModifyResponseAsync(message =>
        {
            message.Embeds = [embed];
        });
    }
}