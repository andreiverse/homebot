using HomeBot.Display;
using HomeBot.Integrations.Jellyfin;
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

        var card = new Card
        {
            Heading = "📺 Jellyfin Server",
            Accent = BrandColors.Jellyfin,
            Content =
            [
                new KeyValueBlock
                {
                    Items =
                    [
                        new()
                        {
                            Key = "Server",
                            Value = sysInfo.ServerName
                        },
                        new()
                        {
                            Key = "Version",
                            Value = sysInfo.Version
                        },
                        new()
                        {
                            Key = "Product",
                            Value = sysInfo.ProductName
                        },
                        new()
                        {
                            Key = "Update",
                            Value = sysInfo.HasUpdateAvailable
                                ? "🟡 Available"
                                : "🟢 Up to date"
                        }
                    ]
                }
            ]
        };

        return card.ToInteractionMessage();
    }

    [SubSlashCommand("itemcounts", "Get Jellyfin item counts")]
    public async Task<InteractionMessageProperties> ItemCounts()
    {
        var counts = await jellyfinIntegration.GetItemCountsAsync();

        var card = new Card
        {
            Heading = "🎬 Jellyfin Library",
            Accent = BrandColors.Jellyfin,
            Content =
            [
                new KeyValueBlock
                {
                    Items =
                    [
                        new()
                        {
                            Key = "🎥 Movies",
                            Value = counts.MovieCount.ToString()
                        },
                        new()
                        {
                            Key = "📺 Series",
                            Value = counts.SeriesCount.ToString()
                        },
                        new()
                        {
                            Key = "🎞️ Episodes",
                            Value = counts.EpisodeCount.ToString()
                        }
                    ]
                }
            ]
        };

        return card.ToInteractionMessage();
    }
}