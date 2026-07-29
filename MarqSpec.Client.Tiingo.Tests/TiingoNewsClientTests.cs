using System.Net;
using System.Text;
using FluentAssertions;
using MarqSpec.Client.Tiingo;

namespace MarqSpec.Client.Tiingo.Tests;

/// <summary>
/// The Tiingo news client, driven entirely against a stubbed transport — no API token, no network. It must send
/// the token as an <c>Authorization: Token</c> header (never the URL, where it leaks) and surface a provider
/// error rather than swallowing it.
/// </summary>
public class TiingoNewsClientTests
{
    private const string Token = "tiingo-token-not-a-secret";

    private static TiingoNewsClient Client(StubHandler handler, string? baseUrl = null) =>
        new(new HttpClient(handler), new TiingoOptions { ApiKey = Token, BaseUrl = baseUrl ?? "https://tiingo.test" });

    private const string TwoArticles = """
        [
          {"id":1,"title":"Fed holds rates","description":"Rates unchanged.","url":"https://ex.com/a","publishedDate":"2026-07-29T12:00:00Z","crawlDate":"2026-07-29T12:05:00Z","source":"reuters.com","tickers":["spy","qqq"],"tags":["Fed"]},
          {"id":2,"title":"Crude draws","description":"Larger draw.","url":"https://ex.com/b","publishedDate":"2026-07-29T13:00:00Z","crawlDate":"2026-07-29T13:05:00Z","source":"eia.gov","tickers":[],"tags":[]}
        ]
        """;

    [Fact]
    public async Task GetNewsAsync_ShouldReturnMappedArticles_WhenTiingoResponds()
    {
        TiingoNewsClient client = Client(new StubHandler { Body = TwoArticles });

        IReadOnlyList<TiingoNewsArticle> articles = await client.GetNewsAsync(DateTimeOffset.UnixEpoch, CancellationToken.None);

        articles.Should().HaveCount(2);
        articles[0].Title.Should().Be("Fed holds rates");
        articles[0].Tickers.Should().BeEquivalentTo(["spy", "qqq"]);
        articles[0].PublishedDate.Should().Be(new DateTimeOffset(2026, 7, 29, 12, 0, 0, TimeSpan.Zero));
        articles[0].Url.Should().Be("https://ex.com/a");
    }

    [Fact]
    public async Task GetNewsAsync_ShouldSendTheTokenAsAnAuthorizationHeader_NeverInTheUrl()
    {
        StubHandler handler = new() { Body = "[]" };

        await Client(handler).GetNewsAsync(new DateTimeOffset(2026, 7, 20, 0, 0, 0, TimeSpan.Zero), CancellationToken.None);

        handler.LastRequest!.Headers.GetValues("Authorization").Should().ContainSingle().Which.Should().Be($"Token {Token}");
        handler.LastRequest.RequestUri!.ToString().Should().NotContain(Token, "a token in the URL leaks into logs and proxies");
        handler.LastRequest.RequestUri.ToString().Should().Contain("startDate=2026-07-20");
    }

    [Fact]
    public async Task GetNewsAsync_ShouldReturnEmpty_WhenTiingoReturnsNull()
    {
        TiingoNewsClient client = Client(new StubHandler { Body = "null" });

        (await client.GetNewsAsync(DateTimeOffset.UnixEpoch, CancellationToken.None)).Should().BeEmpty();
    }

    [Fact]
    public async Task GetNewsAsync_ShouldThrow_WhenTiingoRateLimits()
    {
        TiingoNewsClient client = Client(new StubHandler { Status = HttpStatusCode.TooManyRequests });

        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetNewsAsync(DateTimeOffset.UnixEpoch, CancellationToken.None));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNoApiKeyIsConfigured()
    {
        Action act = () => _ = new TiingoNewsClient(new HttpClient(new StubHandler()), new TiingoOptions { ApiKey = " " });

        act.Should().Throw<InvalidOperationException>();
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        public string Body { get; set; } = "[]";

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(Status)
            {
                Content = new StringContent(Body, Encoding.UTF8, "application/json"),
            });
        }
    }
}
