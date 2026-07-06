using HomeBot.Integrations.Jellyfin;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace HomeBot.Discord.Modules;

[SlashCommand("jellyfin", "Jellyfin related commands")]
public class JellyfinModule(JellyfinIntegration jellyfinIntegration)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("info", "Check Jellyfin server info")]
    public async Task<InteractionMessageProperties> Info()
    {
        var sysInfo = await jellyfinIntegration.GetSystemInfoAsync();

        var embed = new EmbedProperties()
            .WithTitle("📺 Jellyfin Server")
            .WithColor(new Color(0x00A4DC))
            .AddFields(
                new EmbedFieldProperties()
                    .WithName("Server")
                    .WithValue(sysInfo.ServerName)
                    .WithInline(true),

                new EmbedFieldProperties()
                    .WithName("Version")
                    .WithValue(sysInfo.Version)
                    .WithInline(true),

                new EmbedFieldProperties()
                    .WithName("Product")
                    .WithValue(sysInfo.ProductName)
                    .WithInline(true),

                new EmbedFieldProperties()
                    .WithName("Update")
                    .WithValue(sysInfo.HasUpdateAvailable ? "🟡 Available" : "🟢 Up to date")
                    .WithInline(true)
            );

        return new InteractionMessageProperties
        {
            Embeds = new[] { embed }
        };
    }

    [SubSlashCommand("itemcounts", "Get Jellyfin item counts")]
    public async Task<InteractionMessageProperties> ItemCounts()
    {
        var counts = await jellyfinIntegration.GetItemCountsAsync();

        var embed = new EmbedProperties()
            .WithTitle("🎬 Jellyfin Library")
            .WithColor(new Color(0x00A4DC))
            .AddFields(
                new EmbedFieldProperties()
                    .WithName("🎥 Movies")
                    .WithValue(counts.MovieCount.ToString())
                    .WithInline(true),

                new EmbedFieldProperties()
                    .WithName("📺 Series")
                    .WithValue(counts.SeriesCount.ToString())
                    .WithInline(true),

                new EmbedFieldProperties()
                    .WithName("🎞️ Episodes")
                    .WithValue(counts.EpisodeCount.ToString())
                    .WithInline(true)
            );

        return new InteractionMessageProperties
        {
            Embeds = new[] { embed }
        };
    }
}