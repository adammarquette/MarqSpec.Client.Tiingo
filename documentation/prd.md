# PRD — MarqSpec.Client.Tiingo

**Status: the news REST client has shipped.** Price/market-data endpoints are deliberately not in scope.
Tracking issues: [`trading-copilot#440`](https://github.com/adammarquette/trading-copilot/issues/440) (news
client, of [`#383`](https://github.com/adammarquette/trading-copilot/issues/383)),
[`#1171`](https://github.com/adammarquette/trading-copilot/issues/1171) (repo-template backfill, of
[`#701`](https://github.com/adammarquette/trading-copilot/issues/701)).

Ids are appended, never renumbered.

## Purpose

A typed, async .NET client for **Tiingo's news REST API**. Data-only: it fetches news; it never trades. It is
consumed by [trading-copilot](https://github.com/adammarquette/trading-copilot) as one of two news sources
(the other is [`MarqSpec.Client.Finnhub`](https://github.com/adammarquette/MarqSpec.Client.Finnhub)), fanned in
and deduped behind the consumer's `INewsSource` seam.

## Scope

- **News only.** `GET /tiingo/news?startDate=` — the news feed, filterable by date. Ticker/tag filters are
  not exposed on the current public method.
- **Not a price source — by decision.** Tiingo exposes price/market data, but the consumer keeps price data
  **single-source** (Finnhub). Tiingo's activated surface is news; the price endpoints are deliberately not
  implemented here to avoid a duplicate feed.
- **Read-only.** No mutating calls exist on this surface, and none may be added.
- **Typed payloads.** Each endpoint returns strongly-typed records mirroring Tiingo's JSON — the consumer's
  adapter maps them to its venue-neutral `NewsItem`; this client does no normalization or dedup of its own.

## Non-goals

- Order placement, accounts, positions — data source, not execution venue.
- Price/market data — kept single-source in the consumer (Finnhub).
- Dedup / relevance / storage — the consumer's job, not the client's.
- Sharing a public interface with the sibling clients — the venue-neutral symmetry lives in the consumer's seams.

## Requirements

- **R-1 News REST.** `GetNewsAsync` fetches news published on or after a start date, newest first. Shipped
  (gh#440).
- **R-2 Read-only.** The public surface exposes no mutating HTTP verb and no order/account type.
- **R-3 Typed payloads.** `TiingoNewsArticle` mirrors Tiingo's JSON; the consumer normalizes.
- **R-4 net10.0, async, injected transport.** `CancellationToken` on every public async method; `HttpClient`
  injected (not newed); JSON via `System.Text.Json`.
- **R-5 Auth from configuration.** The API token is supplied by the caller (`TiingoOptions`), sourced from the
  consumer's config/environment. **No secret is ever committed here.** Tiingo authenticates with an
  `Authorization: Token` header, never a query-string parameter — a token in a URL leaks into logs and
  proxies.
- **R-6 Free-tier aware.** Rate-limit responses (HTTP 429) surface as exceptions rather than being swallowed,
  so the consumer can degrade to the other source.
- **R-7 Errors are the caller's to handle.** Transport faults and non-success statuses surface; the client
  does not retry silently or block.
- **R-8 Missing credential fails at construction.** A blank `TiingoOptions.ApiKey` throws
  `InvalidOperationException` in the constructor, not on the first call.

## Relationship to the sibling clients

`MarqSpec.Client.Finnhub` is the other data-only news sibling (and the single-source price feed);
`MarqSpec.Client.ProjectX` / `MarqSpec.Client.Tradovate` are the execution-venue siblings. All follow the same
**convention** (typed, async, injected `HttpClient`, config-sourced auth, no shared public interface) so the
consumer's adapters are parallel in shape — each client's signatures are its own, mirroring its provider's
actual API.
