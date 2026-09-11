# MarqSpec.Client.Tiingo

A typed, async .NET client for **Tiingo's news REST API**. It is **data-only**: it fetches news, and does
**not** place orders, hold accounts, or execute anything.

> **Status:** the news client has shipped (`TiingoNewsClient.GetNewsAsync`). Price/market-data endpoints are
> deliberately not built — the consumer keeps price data single-source (Finnhub).

## What this is

A sibling of [`MarqSpec.Client.Finnhub`](https://github.com/adammarquette/MarqSpec.Client.Finnhub) (the other
news source) and the execution clients
[`MarqSpec.Client.ProjectX`](https://github.com/adammarquette/MarqSpec.Client.ProjectX) /
[`MarqSpec.Client.Tradovate`](https://github.com/adammarquette/MarqSpec.Client.Tradovate) — parallel in
convention, **distinct in signatures**. The clients must **not** share a public interface; the venue-neutral
symmetry lives in the consumer's `INewsSource` seam.

**Deliberately news-only — not a price source.** Tiingo also exposes market/price data, but the consumer keeps
**price data single-source** (Finnhub owns equities/indices prices) to avoid duplicate feeds. This is a
decision, not an omission.

**Tracking issues:** news client
[`trading-copilot#440`](https://github.com/adammarquette/trading-copilot/issues/440) (of
[`#383`](https://github.com/adammarquette/trading-copilot/issues/383)). Repo standards backfill:
[`#1171`](https://github.com/adammarquette/trading-copilot/issues/1171) (of
[`#701`](https://github.com/adammarquette/trading-copilot/issues/701)).

## Consumed by

The [trading-copilot](https://github.com/adammarquette/trading-copilot) pins this repo as a git submodule under
`external/` and wraps it in a `TiingoNewsSource : INewsSource` adapter (in a `.Integration.Tiingo` project),
translating Tiingo's payload into the consumer's venue-neutral `NewsItem`. The same story from Tiingo and
Finnhub collapses to one record in the consumer — this client just delivers Tiingo's view.

## Layout

```
MarqSpec.Client.Tiingo/
  MarqSpec.Client.Tiingo/                    # the client library (net10.0)
    TiingoNewsClient.cs                      # news REST — GetNewsAsync
    TiingoNewsArticle.cs                     # raw provider payload
    TiingoOptions.cs                         # API token + base URL; token from config/env, never in source
  MarqSpec.Client.Tiingo.Tests/              # stubbed transport, no token/network
  MarqSpec.Client.Tiingo.IntegrationTests/   # loopback listener, no credentials
  MarqSpec.Client.Tiingo.slnx
  documentation/                             # PRD, architecture, ADRs, agent contracts
  CONTRIBUTING.md · AGENTS.md · LICENSE
```

There is no `Models/` folder and no `.sln` — the article record lives next to the client, and the solution is
`.slnx`.

## Build

```bash
dotnet build MarqSpec.Client.Tiingo.slnx
dotnet format MarqSpec.Client.Tiingo.slnx --verify-no-changes
dotnet test MarqSpec.Client.Tiingo.slnx --filter "Category!=Live"
```

How we work: [`CONTRIBUTING.md`](CONTRIBUTING.md). Agent contracts: [`AGENTS.md`](AGENTS.md). Route docs from
[`documentation/README.md`](documentation/README.md).

## Why a separate repo

Vendored client code lives outside the consumer's `Directory.Build.props`, so a third-party client is not
forced to satisfy the app's house rules, and its release cadence is its own. This is the established
venue-client pattern (ProjectX, Tradovate, Webull, Finnhub).

## License

MIT — see [`LICENSE`](LICENSE).
