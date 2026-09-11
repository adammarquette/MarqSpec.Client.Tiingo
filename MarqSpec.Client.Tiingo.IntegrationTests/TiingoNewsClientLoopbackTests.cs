using System.Net;
using System.Net.Sockets;
using System.Text;
using FluentAssertions;
using MarqSpec.Client.Tiingo;

namespace MarqSpec.Client.Tiingo.IntegrationTests;

/// <summary>
/// News REST against a loopback listener — no Tiingo credential, no public network.
/// Asserts what the listener recorded (R-5: token is an Authorization header, never a URL).
/// </summary>
public sealed class TiingoNewsClientLoopbackTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetNewsAsync_ShouldSendTokenAsAuthorizationHeader_NeverInTheUrl()
    {
        using HttpListener listener = StartListener(out string prefix);
        Task<RecordedRequest> serve = ServeOnceAsync(listener, "[]");

        TiingoNewsClient client = new(
            new HttpClient(),
            new TiingoOptions { ApiKey = "loopback-token-not-a-secret", BaseUrl = prefix.TrimEnd('/') });

        try
        {
            IReadOnlyList<TiingoNewsArticle> articles =
                await client.GetNewsAsync(new DateTimeOffset(2026, 7, 20, 0, 0, 0, TimeSpan.Zero), CancellationToken.None);

            articles.Should().BeEmpty();

            RecordedRequest seen = await serve;
            seen.Authorization.Should().Be("Token loopback-token-not-a-secret");
            seen.RawUrl.Should().NotContain("loopback-token-not-a-secret");
            seen.RawUrl.Should().Contain("startDate=2026-07-20");
        }
        finally
        {
            listener.Stop();
        }
    }

    private static HttpListener StartListener(out string prefix)
    {
        TcpListener probe = new(IPAddress.Loopback, 0);
        probe.Start();
        int port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();

        prefix = $"http://127.0.0.1:{port}/";
        HttpListener listener = new();
        listener.Prefixes.Add(prefix);
        listener.Start();
        return listener;
    }

    private static async Task<RecordedRequest> ServeOnceAsync(HttpListener listener, string body)
    {
        HttpListenerContext context = await listener.GetContextAsync();
        string? authorization = context.Request.Headers["Authorization"];
        string rawUrl = context.Request.RawUrl ?? string.Empty;

        byte[] bytes = Encoding.UTF8.GetBytes(body);
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.ContentType = "application/json";
        context.Response.ContentLength64 = bytes.Length;
        await context.Response.OutputStream.WriteAsync(bytes);
        context.Response.Close();

        return new RecordedRequest(authorization, rawUrl);
    }

    private sealed record RecordedRequest(string? Authorization, string RawUrl);
}
