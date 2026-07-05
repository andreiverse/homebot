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