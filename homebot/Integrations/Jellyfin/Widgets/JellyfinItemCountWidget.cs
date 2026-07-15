using HomeBot.Display;
using HomeBot.Discord;

namespace HomeBot.Integrations.Jellyfin.Widgets;

public class JellyfinItemCountWidget(JellyfinIntegration jellyfin) : IWidget
{
    public string Id => "jellyfin.itemcounts";
    public string Name => "Library Item Counts";
    public string? Description => "Shows Jellyfin library item counts";

    public async Task<Card> RenderAsync(CancellationToken ct = default)
    {
        var counts = await jellyfin.GetItemCountsAsync(ct);

        return new Card()
            .WithHeading("🎬 Jellyfin Library")
            .WithAccent(BrandColors.Jellyfin)
            .AddKeyValueBlock(kv => kv
                .Add("🎥 Movies", counts.MovieCount)
                .Add("📺 Series", counts.SeriesCount)
                .Add("🎞️ Episodes", counts.EpisodeCount)
            );
    }
}