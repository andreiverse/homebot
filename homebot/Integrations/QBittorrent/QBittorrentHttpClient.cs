namespace HomeBot.Integrations.QBittorrent;

public sealed class QBittorrentHttpClient
{
    private readonly HttpClient _http;
    private bool _authenticated;

    public QBittorrentHttpClient(QBittorrentOptions qBittorrentOptions)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(qBittorrentOptions.Endpoint.TrimEnd('/') + "/")
        };
    }

    private Task AuthenticateAsync()
    {
        _authenticated = true;
        if (_authenticated)
            return Task.CompletedTask;

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
        return Task.CompletedTask;
    }

    public async Task<AppVersion> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        await AuthenticateAsync();

        var version = await _http.GetStringAsync("api/v2/app/version", cancellationToken);

        return new AppVersion(version.Trim());
    }

    public async Task<BuildInfo> GetBuildInfoAsync(CancellationToken cancellationToken = default)
    {
        await AuthenticateAsync();

        return await _http.GetFromJsonRequiredAsync<BuildInfo>("api/v2/app/buildInfo", cancellationToken);
    }

    public async Task<Preferences> GetPreferencesAsync(CancellationToken cancellationToken = default)
    {
        await AuthenticateAsync();

        return await _http.GetFromJsonRequiredAsync<Preferences>("api/v2/app/preferences", cancellationToken);
    }

    public async Task<IReadOnlyList<TorrentInfo>> GetTorrentsAsync(
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        await AuthenticateAsync();

        var url = "api/v2/torrents/info";

        if (!string.IsNullOrWhiteSpace(filter))
            url += $"?filter={Uri.EscapeDataString(filter)}";

        return await _http.GetFromJsonAsync<List<TorrentInfo>>(url, cancellationToken)
            ?? [];
    }

    public async Task<TransferInfo> GetTransferInfoAsync(CancellationToken cancellationToken = default)
    {
        await AuthenticateAsync();

        return await _http.GetFromJsonRequiredAsync<TransferInfo>("api/v2/transfer/info", cancellationToken);
    }

}
