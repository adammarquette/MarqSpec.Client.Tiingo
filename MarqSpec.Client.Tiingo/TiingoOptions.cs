namespace MarqSpec.Client.Tiingo;

/// <summary>
/// Configuration for the Tiingo news client. The API token is supplied by the caller and sourced from its own
/// configuration / environment — never hard-coded here.
/// </summary>
public sealed class TiingoOptions
{
    /// <summary>The Tiingo API token. Required; sent as an <c>Authorization: Token</c> header, never in the URL.</summary>
    public string? ApiKey { get; set; }

    /// <summary>The API base address. Overridable for testing; defaults to Tiingo's REST base.</summary>
    public string BaseUrl { get; set; } = "https://api.tiingo.com";
}
