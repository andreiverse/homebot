using HomeBot.Integrations.Jellyfin;
using NetCord.Services.ApplicationCommands;

namespace HomeBot.Discord.Modules;

public class JellyfinModule(JellyfinIntegration jellyfinIntegration)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("jellyfin", "Check Jellyfin server info")]
    public async Task<string> Health()
    {
        var sysInfo = await jellyfinIntegration.GetSystemInfoAsync();

        return $"""
        **{sysInfo.ServerName}**
        Version: {sysInfo.Version}
        Product Name: {sysInfo.ProductName}
        Update Available: {sysInfo.HasUpdateAvailable}
        """;
    }
}