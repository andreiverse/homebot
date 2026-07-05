
using Microsoft.Extensions.Options;

namespace HomeBot.Integrations.Jellyfin;

public class JellyfinIntegration : BaseIntegration<JellyfinIntegration>
{
    private readonly JellyfinHttpClient _client;

    public JellyfinIntegration(IOptionsMonitor<JellyfinOptions> options, 
                                ILogger<JellyfinIntegration> logger) : base(
        logger,
        new IntegrationMetadata("Jellyfin", "Jellyfin integration")
    )
    {
        _client = new JellyfinHttpClient(options.CurrentValue.Endpoint, options.CurrentValue.ApiKey);
    
        logger.LogInformation("Jellyfin integration initiated successfully");
    }

    public async Task<SystemInfo> GetSystemInfoAsync()
    {
        return await _client.GetSystemInfoAsync();
    }

}