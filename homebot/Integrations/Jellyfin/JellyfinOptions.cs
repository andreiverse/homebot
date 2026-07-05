public sealed class JellyfinOptions
{
    public const string Section = "Jellyfin";

    public required string Endpoint { get; init; }

    public required string ApiKey { get; init; }
}