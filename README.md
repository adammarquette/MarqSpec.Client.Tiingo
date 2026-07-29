# MarqSpec.Client.Tiingo

A .NET client library for the **Tiingo news REST API** — a **data-only** news source.

> **Status: the news client has shipped** (gh#440). `TiingoNewsClient.GetNewsAsync` fetches news over REST,
> filtered from a start date, with its own test suite. Price endpoints are deliberately not built — price data
> stays single-source (Finnhub).

## What this is

A typed, async .NET client for **Tiingo's news feed** — the news REST endpoint — returning raw provider payloads
for a consumer to normalize. It is **data-only**: it fetches news, and does **not** place orders, hold accounts,
or execute anything (the R-17 data-source / execution-venue split).

It is a **sibling** of [`MarqSpec.Client.Finnhub`](https://github.com/adammarquette/MarqSpec.Client.Finnhub) (the
other news source) and the execution clients
[`MarqSpec.Client.ProjectX`](https://github.com/adammarquette/MarqSpec.Client.ProjectX) /
[`MarqSpec.Client.Tradovate`](https://github.com/adammarquette/MarqSpec.Client.Tradovate) — parallel in
convention, **distinct in signatures**. The clients must **not** share a public interface; the venue-neutral
symmetry lives in the consumer's `INewsSource` seam.

**Deliberately news-only — not a price source.** Tiingo also exposes market/price data, but the consumer keeps
**price data single-source** (Finnhub owns equities/indices prices) to avoid duplicate feeds. Tiingo's activated
surface here is **news**, and only news. This is a decision, not an omission.

**Tracking issue:** [`adammarquette/trading-copilot#383`](https://github.com/adammarquette/trading-copilot/issues/383)

## Consumed by

The [trading-copilot](https://github.com/adammarquette/trading-copilot) pins this repo as a git submodule under
`external/` and wraps it in a `TiingoNewsSource : INewsSource` adapter (in a `.Integration.Tiingo` project),
translating Tiingo's payload into the consumer's venue-neutral `NewsItem`. The same story from Tiingo and Finnhub
collapses to one record in the consumer — this client just delivers Tiingo's view.

## Planned layout

```
MarqSpec.Client.Tiingo/
  MarqSpec.Client.Tiingo/            # the client library (net10.0)
    TiingoNewsClient.cs             # the typed REST client — GetNewsAsync (tickers / tags / since)
    TiingoOptions.cs               # API token + base URL; token from config/env, never in source
    Models/                        # the raw payload records (news article)
  MarqSpec.Client.Tiingo.sln
  PRD.md · README.md · LICENSE
```

## Why a separate repo

Vendored client code lives outside the consumer's `Directory.Build.props` (net10-only, warnings-as-errors), so a
third-party client is not forced to satisfy the app's house rules, and its release cadence is its own. This is
the established venue-client pattern (ProjectX, Tradovate, Webull, Finnhub).

## License

MIT — see [`LICENSE`](LICENSE).
