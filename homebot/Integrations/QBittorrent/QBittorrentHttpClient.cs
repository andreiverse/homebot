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