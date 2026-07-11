using HomeBot.Display;
using HomeBot.Integrations.QBittorrent;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace HomeBot.Discord.Modules;

[SlashCommand("qbittorrent", "qBittorrent related commands")]
public class QBittorrentModule(QBittorrentIntegration qbittorrent)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("info", "Show qBittorrent information")]
    public async Task<InteractionMessageProperties> Info()
    {
        var version = await qbittorrent.GetVersionAsync();
        var build = await qbittorrent.GetBuildInfoAsync();

        var card = new Card
        {
            Heading = "⬇️ qBittorrent",
            Accent = BrandColors.QBittorrent,
            Content =
            [
                new KeyValueBlock
                {
                    Items =
                    [
                        new() { Key = "Version", Value = version.Version },
                        new() { Key = "Qt", Value = build.Qt },
                        new() { Key = "libtorrent", Value = build.Libtorrent },
                        new() { Key = "OpenSSL", Value = build.OpenSSL },
                        new() { Key = "Architecture", Value = $"{build.Bitness}-bit" }
                    ]
                }
            ]
        };

        return card.ToInteractionMessage();
    }

    [SubSlashCommand("transfers", "Show transfer statistics")]
    public async Task<InteractionMessageProperties> Transfers()
    {
        var info = await qbittorrent.GetTransferInfoAsync();

        var card = new Card
        {
            Heading = "📊 Transfer Statistics",
            Accent = BrandColors.QBittorrent,
            Content =
            [
                new KeyValueBlock
                {
                    Items =
                    [
                        new() { Key = "⬇ Download", Value = $"{FormatSize(info.DlInfoSpeed)}/s" },
                        new() { Key = "⬆ Upload", Value = $"{FormatSize(info.UpInfoSpeed)}/s" },
                        new() { Key = "Downloaded", Value = FormatSize(info.DlInfoData) },
                        new() { Key = "Uploaded", Value = FormatSize(info.UpInfoData) },
                        new() { Key = "Connection", Value = info.ConnectionStatus },
                        new() { Key = "DHT Nodes", Value = info.DhtNodes.ToString() }
                    ]
                }
            ]
        };

        return card.ToInteractionMessage();
    }

    [SubSlashCommand("torrents", "List all torrents")]
    public Task<InteractionMessageProperties> Torrents()
        => TorrentList(null, "📦 Torrents");

    [SubSlashCommand("downloading", "List downloading torrents")]
    public Task<InteractionMessageProperties> Downloading()
        => TorrentList("downloading", "⬇ Downloading");

    [SubSlashCommand("completed", "List completed torrents")]
    public Task<InteractionMessageProperties> Completed()
        => TorrentList("completed", "✅ Completed");

    [SubSlashCommand("paused", "List paused torrents")]
    public Task<InteractionMessageProperties> Paused()
        => TorrentList("paused", "⏸ Paused");

    [SubSlashCommand("active", "List active torrents")]
    public Task<InteractionMessageProperties> Active()
        => TorrentList("active", "🟢 Active");

    [SubSlashCommand("inactive", "List inactive torrents")]
    public Task<InteractionMessageProperties> Inactive()
        => TorrentList("inactive", "⚪ Inactive");

    private async Task<InteractionMessageProperties> TorrentList(
        string? filter,
        string heading)
    {
        var torrents = await qbittorrent.GetTorrentsAsync(filter);

        var card = new Card
        {
            Heading = $"{heading} ({torrents.Count})",
            Accent = BrandColors.QBittorrent
        };

        if (torrents.Count == 0)
        {
            card.Content.Add(new TextBlock
            {
                Text = "No torrents found."
            });
        }
        else
        {
            foreach (var torrent in torrents.Take(6))
            {
                card.Content.Add(new KeyValueBlock
                {
                    Items =
                    [
                        new()
                    {
                        Key = torrent.Name,
                        Value =
                            $"**{torrent.Progress:P0}** • {torrent.State}\n" +
                            $"⬇ {FormatSize(torrent.Dlspeed)}/s • ⬆ {FormatSize(torrent.Upspeed)}/s\n" +
                            $"🌱 {torrent.NumSeeds} • 🪱 {torrent.NumLeechs} • Ratio {torrent.Ratio:0.00}"
                    }
                    ]
                });
            }

            if (torrents.Count > 6)
            {
                card.Content.Add(new TextBlock
                {
                    Text = $"Showing **6** of **{torrents.Count}** torrents."
                });
            }
        }

        return card.ToInteractionMessage();
    }

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];

        double size = bytes;
        int unit = 0;

        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.#} {units[unit]}";
    }
}