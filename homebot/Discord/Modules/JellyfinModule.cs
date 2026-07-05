using HomeBot.Integrations.Jellyfin;
using NetCord.Services.ApplicationCommands;

namespace HomeBot.Discord.Modules;

[SlashCommand("jellyfin", "Jellyfin related commands")]
public class JellyfinModule(JellyfinIntegration jellyfinIntegration)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("info", "Check Jellyfin server info")]
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

    [SubSlashCommand("itemcounts", "Get jellyfin item counts")]
    public async Task<String> ItemCounts()
    {
        var itemCounts = await jellyfinIntegration.GetItemCountsAsync();

        return $"""
        **Item counts**
        Movies: {itemCounts.MovieCount}
        Series: {itemCounts.SeriesCount} (total episodes: {itemCounts.EpisodeCount})
        """;
    }
}