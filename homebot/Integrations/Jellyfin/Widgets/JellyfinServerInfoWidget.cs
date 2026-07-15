using HomeBot.Display;
using HomeBot.Discord;

namespace HomeBot.Integrations.Jellyfin.Widgets;

public class JellyfinServerInfoWidget(JellyfinIntegration jellyfin) : IWidget
{
    public string Id => "jellyfin.serverinfo";
    public string Name => "Server Info";
    public string? Description => "Shows useful Jellyfin server info";

    public async Task<Card> RenderAsync(CancellationToken ct = default)
    {
        var sysInfo = await jellyfin.GetSystemInfoAsync(ct);

        return new Card()
            .WithHeading("📺 Jellyfin Server")
            .WithAccent(BrandColors.Jellyfin)
            .AddKeyValueBlock(kv => kv
                .Add("Server", sysInfo.ServerName)
                .Add("Version", sysInfo.Version)
                .Add("Product", sysInfo.ProductName)
                .Add("Update", sysInfo.HasUpdateAvailable ? "🟡 Available" : "🟢 Up to date")
            );
    }
}