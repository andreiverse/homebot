namespace HomeBot.Integrations.Jellyfin;

public class JellyfinOptions : IIntegrationOptions
{
    public static string Section => "Jellyfin";
    public bool Enabled { get; init; } = false;

    public required string Endpoint { get; init; }
    public required string ApiKey { get; init; }
    
}