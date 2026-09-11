# Documentation

**This directory is authoritative. Do not read it wholesale.** Find the document you need here, open **the
section you need**, and stop. `R-#`, ADR numbers and `gh#N` are the symbol table — resolve symbols on demand,
the way a compiler does, rather than loading every source file.

Sizes are approximate tokens, so a reader can see what a read costs before paying for it. **Keep them roughly
accurate** — a size column nobody updates is worse than none, because it is trusted.

## Start here

| Document | ~tok | Read it when |
|---|---|---|
| [`prd.md`](prd.md) | 0.9K | You need to know **what is required**, or you are citing an `R-#`. Ids are stable and never renumbered. |
| [`architecture.md`](architecture.md) | 0.8K | Before changing the shape — the request path, the auth flow, DI lifetimes. |
| [`adr/`](adr/README.md) | index | You are about to change something and want to know whether the current shape was chosen or inherited. **Never read the folder** — resolve the number. |

## Working agreements

| Document | ~tok | Read it when |
|---|---|---|
| [`AGENT-MEMORY.md`](AGENT-MEMORY.md) | 0.4K | **Before starting any work.** Cheap; just read it. |
| [`agents/`](agents/README.md) | index | You are wearing a role hat. Reviewer, Platform and Coordinator contracts **never auto-load** — open them yourself. |
| [`../CONTRIBUTING.md`](../CONTRIBUTING.md) | 2.0K | Branching, claiming, commits, PRs, and the release procedure. |
| [`../AGENTS.md`](../AGENTS.md) | 1.4K | Loads automatically. The non-negotiables and the role routing table. |

## Role contracts — `agents/`

Loaded **on demand by role**, not by directory.

| Contract | Open it when |
|---|---|
| [`code-reviewer.md`](agents/code-reviewer.md) | You are reviewing a change — **anywhere** in the repo |
| [`platform.md`](agents/platform.md) | You are touching CI/CD, packaging, or the release path |
| [`coordinator.md`](agents/coordinator.md) | You are assigning work from the board, or driving a task to approval |

Two more load by directory proximity from the subtree they govern: the **Coding** contract at
[`../MarqSpec.Client.Tiingo/AGENTS.md`](../MarqSpec.Client.Tiingo/AGENTS.md) and the **QA** contract at
[`../MarqSpec.Client.Tiingo.IntegrationTests/AGENTS.md`](../MarqSpec.Client.Tiingo.IntegrationTests/AGENTS.md).
[`agents/README.md`](agents/README.md) explains why the five split that way.

## What is not here

- **Task specs and acceptance criteria.** They live in the **GitHub issue**. A spec here duplicates the
  tracker and drifts from it.
- **API reference and usage examples.** They live in the root README, which ships inside the NuGet
  package — so nuget.org and the repo cannot disagree.

---
*Adding a document? Add its row here in the same PR — a document nothing routes to is a document nobody opens.*
