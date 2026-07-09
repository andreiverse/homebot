namespace HomeBot.Integrations.Jellyfin;

public sealed class JellyfinHttpClient
{
    private readonly HttpClient _http;

    public JellyfinHttpClient(string endpoint, string apiKey)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(endpoint)
        };

        _http.DefaultRequestHeaders.Add("X-Emby-Token", apiKey);
    }

    public async Task<SystemInfo> GetSystemInfoAsync()
    {
        return await _http.GetFromJsonAsync<SystemInfo>("/System/Info")
               ?? throw new InvalidOperationException("Failed to retrieve system info.");
    }

    public async Task<ItemCounts> GetItemCountsAsync()
    {
        return await _http.GetFromJsonAsync<ItemCounts>("/Items/Counts")
            ?? throw new InvalidOperationException("Failed to retrive item counts");
    }
}