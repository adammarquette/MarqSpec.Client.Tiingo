using System.Text.Json.Serialization;

namespace MarqSpec.Client.Tiingo;

/// <summary>
/// One news article as Tiingo's <c>/tiingo/news</c> endpoint returns it — the raw provider shape, before any
/// normalization. A consumer maps this to its own domain type.
/// </summary>
/// <param name="Id">Tiingo's numeric article id.</param>
/// <param name="Title">The article title.</param>
/// <param name="Description">The article summary / body text.</param>
/// <param name="Url">The article URL.</param>
/// <param name="PublishedDate">Publication time as an ISO-8601 timestamp.</param>
/// <param name="CrawlDate">When Tiingo crawled the article (ISO-8601).</param>
/// <param name="Source">The publishing source domain (e.g. <c>reuters.com</c>).</param>
/// <param name="Tickers">The tickers Tiingo tagged — a JSON array, already split (unlike Finnhub's CSV).</param>
/// <param name="Tags">Tiingo's topic tags.</param>
public sealed record TiingoNewsArticle(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("publishedDate")] DateTimeOffset PublishedDate,
    [property: JsonPropertyName("crawlDate")] DateTimeOffset CrawlDate,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("tickers")] IReadOnlyList<string>? Tickers,
    [property: JsonPropertyName("tags")] IReadOnlyList<string>? Tags);
