using System.Net;

namespace HomeBot.Integrations.QBittorrent;

public sealed class QBittorrentHttpClient
{
    private readonly HttpClient _http;
    private bool _authenticated;

    public QBittorrentHttpClient(
        string endpoint,
        string username,
        string password)
    {
        var cookies = new CookieContainer();

        var handler = new HttpClientHandler
        {
            CookieContainer = cookies,
            UseCookies = true
        };

        _http = new HttpClient(handler)
        {
            BaseAddress = new Uri(endpoint.TrimEnd('/') + "/")
        };

        Username = username;
        Password = password;
    }

    private string Username { get; }
    private string Password { get; }

    private async Task AuthenticateAsync()
    {
        _authenticated = true;
        // if (_authenticated)
        //     return;

        // using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        // {
        //     ["username"] = Username,
        //     ["password"] = Password
        // });

        // var response = await _http.PostAsync("api/v2/auth/login", content);
        // response.EnsureSuccessStatusCode();

        // var body = await response.Content.ReadAsStringAsync();

        // if (!body.Trim().Equals("Ok.", StringComparison.OrdinalIgnoreCase))
        //     throw new InvalidOperationException("Authentication failed.");

        // _authenticated = true;
    }

    public async Task<AppVersion> GetVersionAsync()
    {
        await AuthenticateAsync();

        var version = await _http.GetStringAsync("api/v2/app/version");

        return new AppVersion(version.Trim());
    }

    public async Task<BuildInfo> GetBuildInfoAsync()
    {
        await AuthenticateAsync();

        return await _http.GetFromJsonAsync<BuildInfo>("api/v2/app/buildInfo")
            ?? throw new InvalidOperationException("Failed to retrieve build info.");
    }

    public async Task<Preferences> GetPreferencesAsync()
    {
        await AuthenticateAsync();

        return await _http.GetFromJsonAsync<Preferences>("api/v2/app/preferences")
            ?? throw new InvalidOperationException("Failed to retrieve preferences.");
    }

    public async Task<SearchStartResponse> StartSearchAsync(
        string pattern,
        IEnumerable<string>? plugins = null,
        string category = "all")
    {
        await AuthenticateAsync();

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["pattern"] = pattern,
            ["category"] = category,
            ["plugins"] = plugins is null
                ? "enabled"
                : string.Join("|", plugins)
        });

        var response = await _http.PostAsync("api/v2/search/start", content);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SearchStartResponse>()
            ?? throw new InvalidOperationException("Failed to start search.");
    }

    public async Task<IReadOnlyList<TorrentInfo>> GetTorrentsAsync(
        string? filter = null)
    {
        await AuthenticateAsync();

        var url = "api/v2/torrents/info";

        if (!string.IsNullOrWhiteSpace(filter))
            url += $"?filter={Uri.EscapeDataString(filter)}";

        return await _http.GetFromJsonAsync<List<TorrentInfo>>(url)
            ?? [];
    }

    public async Task<TransferInfo> GetTransferInfoAsync()
    {
        await AuthenticateAsync();

        return await _http.GetFromJsonAsync<TransferInfo>("api/v2/transfer/info")
            ?? throw new InvalidOperationException();
    }

    public async Task<IReadOnlyList<SearchResult>> GetSearchResultsAsync(
        int searchId,
        int limit = 100,
        int offset = 0)
    {
        await AuthenticateAsync();

        var response = await _http.GetFromJsonAsync<SearchResultsResponse>(
        $"api/v2/search/results?id={searchId}&limit={limit}&offset={offset}")
        ?? throw new InvalidOperationException("Failed to retrieve search results.");

        return response.Results;
    }

    public async Task<SearchStatus> GetSearchStatusAsync(int searchId)
    {
        await AuthenticateAsync();

        var statuses = await _http.GetFromJsonAsync<List<SearchStatus>>(
                           $"api/v2/search/status?id={searchId}")
                       ?? throw new InvalidOperationException("Failed to retrieve search status.");

        return statuses.Single();
    }

    public async Task DeleteSearchAsync(int searchId)
    {
        await AuthenticateAsync();

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = searchId.ToString()
        });

        var response = await _http.PostAsync("api/v2/search/delete", content);
        response.EnsureSuccessStatusCode();
    }
}