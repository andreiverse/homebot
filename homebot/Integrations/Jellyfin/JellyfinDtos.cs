namespace HomeBot.Integrations.Jellyfin;

public sealed class ItemCounts
{
    public int MovieCount { get; set; }
    public int SeriesCount { get; set; }

    public int EpisodeCount { get; set; }
}

public sealed class SystemInfo
{
    public string ServerName { get; set; } = "";
    public string Version { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string OperatingSystemDisplayName { get; set; } = "";
    public bool HasUpdateAvailable { get; set; }
    public bool HasPendingRestart { get; set; }
    public bool IsShuttingDown { get; set; }
}
