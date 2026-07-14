using HomeBot.Display;
using HomeBot.Integrations.Jellyfin;
using HomeBot.Integrations.Jellyfin.Widgets;
using Microsoft.Extensions.ObjectPool;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace HomeBot.Discord.Modules;

[SlashCommand("jellyfin", "Jellyfin related commands")]
public class JellyfinModule(JellyfinIntegration jellyfinIntegration, JellyfinServerInfoWidget jellyfinServerInfoWidget)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("info", "Check Jellyfin server info")]
    public async Task<InteractionMessageProperties> Info()
    { 
        return (await jellyfinServerInfoWidget.RenderAsync(jellyfinIntegration, new CancellationToken())).ToInteractionMessage();
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