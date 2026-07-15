using HomeBot.Display;
using HomeBot.Integrations.Jellyfin.Widgets;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace HomeBot.Discord.Modules;

[SlashCommand("jellyfin", "Jellyfin related commands")]
public class JellyfinModule(
    JellyfinServerInfoWidget jellyfinServerInfoWidget,
    JellyfinItemCountWidget jellyfinItemCountWidget)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("info", "Check Jellyfin server info")]
    public async Task<InteractionMessageProperties> Info()
        => (await jellyfinServerInfoWidget.RenderAsync()).ToInteractionMessage();

    [SubSlashCommand("itemcounts", "Get Jellyfin item counts")]
    public async Task<InteractionMessageProperties> ItemCounts()
        => (await jellyfinItemCountWidget.RenderAsync()).ToInteractionMessage();
}