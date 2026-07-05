using HomeBot.Integrations.Jellyfin;
using NetCord.Services.ApplicationCommands;

namespace HomeBot.Commands;

public class JellyfinCommands(JellyfinIntegration jellyfinIntegration)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("jellyfin", "Check Jellyfin server info")]
    public async Task<string> Health()
    {
        var sysInfo = await jellyfinIntegration.GetSystemInfoAsync();

        return $"""
        **{sysInfo.ServerName}**
        Version: {sysInfo.Version}
        OS: {sysInfo.OperatingSystemDisplayName}
        Update Available: {sysInfo.HasUpdateAvailable}
        """;
    }
}