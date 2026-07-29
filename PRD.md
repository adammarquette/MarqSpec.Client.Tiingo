# PRD — MarqSpec.Client.Tiingo

**Status: requirements only.** This document is the plan; the repo holds no implementation yet
(scaffolding, per the venue-client pattern). Tracking issue:
[`adammarquette/trading-copilot#383`](https://github.com/adammarquette/trading-copilot/issues/383).

## Purpose

A typed, async .NET client for **Tiingo's news REST API**. Data-only: it fetches news; it never trades. It is
consumed by the [trading-copilot](https://github.com/adammarquette/trading-copilot) as one of two news sources
(the other is [`MarqSpec.Client.Finnhub`](https://github.com/adammarquette/MarqSpec.Client.Finnhub)), fanned in
and deduped behind the consumer's `INewsSource` seam.

## Scope

- **News only.** `GET /tiingo/news?tickers=&tags=&startDate=` — the news feed, filterable by ticker/tag/date.
- **Not a price source — by decision.** Tiingo exposes price/market data, but the consumer keeps price data
  **single-source** (Finnhub). Tiingo's activated surface is news; the price endpoints are deliberately not
  implemented here to avoid a duplicate feed.
- **Read-only.** No mutating calls, and none may be added.
- **Typed payloads.** Each endpoint returns strongly-typed records mirroring Tiingo's JSON — the consumer's
  adapter maps them to its venue-neutral `NewsItem`; this client does no normalization or dedup of its own.

## Non-goals

- Order placement, accounts, positions — data source, not execution venue (R-17).
- Price/market data — kept single-source in the consumer (Finnhub).
- Dedup / relevance / storage — the consumer's job, not the client's.
- Sharing a public interface with the sibling clients — the venue-neutral symmetry lives in the consumer's seams.

## Requirements

- **`net10.0`**, async-all-the-way with `CancellationToken`, `HttpClient` injected (not newed), typed via
  `System.Text.Json`.
- **Auth from configuration** — the API token is supplied by the caller (`TiingoOptions`), sourced from the
  consumer's config/environment. Tiingo authenticates via a token header/query param; **no secret is ever
  committed here.**
- **Free-tier aware.** Tiingo's free tier is rate-limited; the client surfaces provider errors and rate-limit
  responses to the caller rather than swallowing them, so the consumer can degrade to the other source.
- **Errors are the caller's to handle.** Transport faults and non-success statuses surface as typed
  exceptions/results; the client does not retry silently or block.

## Relationship to the sibling clients

`MarqSpec.Client.Finnhub` is the other data-only news sibling; `MarqSpec.Client.ProjectX` /
`MarqSpec.Client.Tradovate` are the execution-venue siblings. All follow the same **convention** (typed, async,
injected `HttpClient`, config-sourced auth, no shared public interface) so the consumer's adapters are parallel in
shape — each client's signatures are its own, mirroring its provider's actual API.
