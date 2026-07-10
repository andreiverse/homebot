namespace HomeBot.Integrations.QBittorrent;

public sealed record AppVersion(string Version);

public sealed record BuildInfo(
    string Qt,
    string Libtorrent,
    string Boost,
    string OpenSSL,
    int Bitness);

public sealed record Preferences(
    string Locale,
    int MaxActiveDownloads,
    int MaxActiveUploads);

public sealed record SearchStartResponse(int Id);

public sealed record SearchStatus(
    int Id,
    string Status,
    int Total);

public sealed record TorrentInfo(
    string Hash,
    string Name,
    long Size,
    double Progress,
    long Dlspeed,
    long Upspeed,
    string State,
    int NumSeeds,
    int NumLeechs,
    double Ratio,
    int Eta,
    string Category);

public sealed record TransferInfo(
    long DlInfoSpeed,
    long UpInfoSpeed,
    long DlInfoData,
    long UpInfoData,
    string ConnectionStatus,
    int DhtNodes);

