using Microsoft.Extensions.Options;

namespace HomeBot.Integrations.Jellyfin;

public sealed class JellyfinHttpClient
{
    private readonly HttpClient _http;

    public JellyfinHttpClient(JellyfinOptions options)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(options.Endpoint)
        };
        _http.DefaultRequestHeaders.Add("X-Emby-Token", options.ApiKey);
    }

    public Task<SystemInfo> GetSystemInfoAsync(CancellationToken cancellationToken = default)
        => _http.GetFromJsonRequiredAsync<SystemInfo>("/System/Info", cancellationToken);

    public Task<ItemCounts> GetItemCountsAsync(CancellationToken cancellationToken = default)
        => _http.GetFromJsonRequiredAsync<ItemCounts>("/Items/Counts", cancellationToken);
}
