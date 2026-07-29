using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace MarqSpec.Client.Tiingo;

/// <summary>The Tiingo news REST surface.</summary>
public interface ITiingoNewsClient
{
    /// <summary>Fetches news published on or after <paramref name="startDate"/>, newest first.</summary>
    /// <param name="startDate">The earliest publication date to return.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The articles Tiingo returned.</returns>
    Task<IReadOnlyList<TiingoNewsArticle>> GetNewsAsync(DateTimeOffset startDate, CancellationToken cancellationToken);
}

/// <summary>
/// A typed, async client for Tiingo's news REST API. Data-only: it fetches news and nothing else — Tiingo's
/// price surface is deliberately not implemented (price data stays single-source, Finnhub). Errors are the
/// caller's: a non-success status or transport fault surfaces as an exception rather than being swallowed, so a
/// consumer can degrade to another source. Rate-limit responses (HTTP 429) surface the same way.
/// </summary>
/// <remarks>
/// The API token authenticates via an <c>Authorization: Token &lt;key&gt;</c> header, never a query-string
/// parameter — a token in a URL leaks into logs and proxies.
/// </remarks>
public sealed class TiingoNewsClient : ITiingoNewsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly TiingoOptions _options;

    /// <summary>Creates the client.</summary>
    /// <param name="httpClient">The HTTP client (injected, so timeouts / handlers are the host's to configure).</param>
    /// <param name="options">The Tiingo configuration, including the API token.</param>
    /// <exception cref="InvalidOperationException">No API token is configured.</exception>
    public TiingoNewsClient(HttpClient httpClient, TiingoOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _httpClient = httpClient;
        _options = options;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Tiingo API token is not configured (TiingoOptions.ApiKey).");
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TiingoNewsArticle>> GetNewsAsync(DateTimeOffset startDate, CancellationToken cancellationToken)
    {
        // Tiingo's startDate filter is date-granular (yyyy-MM-dd); the consumer re-filters by the exact instant.
        string start = startDate.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        Uri endpoint = new(new Uri(_options.BaseUrl.TrimEnd('/') + "/"), $"tiingo/news?startDate={start}");

        using HttpRequestMessage request = new(HttpMethod.Get, endpoint);
        request.Headers.Add("Authorization", $"Token {_options.ApiKey}");

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        IReadOnlyList<TiingoNewsArticle>? articles =
            await response.Content.ReadFromJsonAsync<IReadOnlyList<TiingoNewsArticle>>(JsonOptions, cancellationToken);

        return articles ?? [];
    }
}
