namespace HomeBot.Integrations.Jellyfin;

public sealed class JellyfinHttpClient
{
    private readonly HttpClient _http;

    public JellyfinHttpClient(HttpClient http)
    {
        _http = http;
    }

    public Task<SystemInfo> GetSystemInfoAsync(CancellationToken cancellationToken = default)
        => _http.GetFromJsonRequiredAsync<SystemInfo>("/System/Info", cancellationToken);

    public Task<ItemCounts> GetItemCountsAsync(CancellationToken cancellationToken = default)
        => _http.GetFromJsonRequiredAsync<ItemCounts>("/Items/Counts", cancellationToken);
}
