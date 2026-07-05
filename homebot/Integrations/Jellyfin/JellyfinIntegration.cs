
namespace HomeBot.Integrations.Jellyfin;

public class JellyfinIntegration : BaseIntegration
{
    private readonly JellyfinHttpClient _client;

    public JellyfinIntegration(String endpoint, String apiKey) : base(
        new IntegrationMetadata("Jellyfin", "Jellyfin integration")
    )
    {
        _client = new JellyfinHttpClient(endpoint, apiKey);
    }

    public async Task<SystemInfo> GetSystemInfoAsync()
    {
        return await _client.GetSystemInfoAsync();
    }

}