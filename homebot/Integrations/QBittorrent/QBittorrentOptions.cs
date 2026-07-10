namespace HomeBot.Integrations.QBittorrent;

public class QBittorrentOptions : IIntegrationOptions
{
    public static string Section => "QBittorrent";
    public bool Enabled { get; init; } = false;

    public required string Endpoint { get; init; }
}