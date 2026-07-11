namespace HomeBot.Discord;

/// <summary>
/// Named accent colors used by <c>Card</c>s built in Discord modules, so integration
/// brand colors and status colors aren't repeated as magic hex strings per command.
/// </summary>
public static class BrandColors
{
    public const string Jellyfin = "#00A4DC";
    public const string Prometheus = "#E6522C";
    public const string QBittorrent = "#2F67BA";
    public const string Metrics = "#5865F2";

    public const string HealthHealthy = "#57F287";
    public const string HealthUnknown = "#FEE75C";
    public const string HealthUnhealthy = "#ED4245";

    public const string AlertsClear = "#43B581";
    public const string AlertsFiring = "#F04747";
}
