using System.Net.Http.Json;

namespace HomeBot.Integrations;

public static class HttpClientJsonExtensions
{
    /// <summary>
    /// Like <see cref="HttpClientJsonExtensions.GetFromJsonAsync{TValue}"/>, but throws
    /// instead of returning null for an empty/"null" JSON body.
    /// </summary>
    public static async Task<T> GetFromJsonRequiredAsync<T>(
        this HttpClient http,
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<T>(requestUri, cancellationToken)
            ?? throw new InvalidOperationException($"No response from {requestUri}.");
    }
}
