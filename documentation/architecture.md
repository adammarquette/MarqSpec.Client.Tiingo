# Architecture — MarqSpec.Client.Tiingo

How the library is put together, and why. Requirements are in the [PRD](prd.md) (`R-#`); decisions and their
alternatives are in [`adr/`](adr/README.md).

## The one-paragraph version

A consumer constructs `TiingoNewsClient` with an injected `HttpClient` and `TiingoOptions` (API token + base
URL). There is no registration extension — the consumer's adapter owns DI. Auth is an
`Authorization: Token` header. Failures surface; nothing is retried here.

## Composition

```
Consumer adapter (trading-copilot .Integration.Tiingo)
├── TiingoOptions               section from the host; token from env
├── HttpClient                  host-configured (timeouts, handlers)
└── TiingoNewsClient            news REST
```

**This library does not register itself.** Lifetimes are the consumer's: the HTTP client is typically
typed/`AddHttpClient` in the host. A lifetime chosen here would sit below the host's gate and could not be
audited there.

## The request path

**News (R-1).** `GetNewsAsync` formats the start date as `yyyy-MM-dd` (Tiingo's filter is date-granular; the
consumer re-filters by the exact instant), builds `GET {BaseUrl}/tiingo/news?startDate=…`, adds
`Authorization: Token {ApiKey}`, sends, `EnsureSuccessStatusCode`, deserializes `TiingoNewsArticle[]`. A
null JSON body becomes an empty list. A 429 is an `HttpRequestException`.

The token never appears in the URL (R-5).

## Failure semantics

- **Nothing is retried in this library.** A timeout is an unknown outcome the caller owns.
- Rate limits and other non-success statuses surface as `HttpRequestException` via `EnsureSuccessStatusCode`.
- A missing API token fails at construction (`InvalidOperationException`), not on the first call (R-8).

## The consumer boundary

| This library provides | The consumer provides |
|---|---|
| Transport, serialization, header auth | Policy, limits, orchestration, DI |
| Typed models | `NewsItem` domain types |
| Raw Tiingo payloads | Dedup, relevance, storage, ticker/tag filtering |

## Testing shape

| Tier | Project | Backing | Runs |
|---|---|---|---|
| Unit | `MarqSpec.Client.Tiingo.Tests` | stubbed `HttpMessageHandler`, no I/O | always, in seconds |
| Integration | `MarqSpec.Client.Tiingo.IntegrationTests` | loopback `HttpListener`, no credentials | always |
| Live | same project, `Category=Live` | the real Tiingo API | opt-in only; none yet |

## Known shape issues

- Ticker and tag query parameters exist on Tiingo's news endpoint and are not exposed on `GetNewsAsync`.
  Adding them is a surface change, not a cleanup.
- News 429s are raw `HttpRequestException`, not a typed rate-limit type. Do not "fix" that without an ADR;
  the consumer already branches on HTTP failures.
- There is no `AddTiingo…` extension. Adding one is a surface change, not a cleanup.
- The library is `net10.0` only. Multi-targeting would be a new ADR, not an assumption of the template.
- Price/market-data endpoints are absent by decision (see the PRD *Non-goals*), not because the work is
  unfinished.
