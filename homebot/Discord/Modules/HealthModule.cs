using HomeBot.Integrations;
using NetCord.Services.ApplicationCommands;

namespace HomeBot.Discord.Modules;

public class HealthModule(IntegrationManager integrationManager) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("health", "Check integration health health")]
    public async Task<string> Health()
    {
        var res = "**Integration Health**";
        var healthz = await integrationManager.PerformHealthChecks();
        foreach (var kvp in healthz)
        {
            res += $"\n{kvp.Key.Name}: {kvp.Value.ToString()}";
        }

        return res;
    }
}
