# Source code agent guidelines

These instructions apply to `src/`. Read root `AGENTS.md` first.

## Current clients

- `HF.Mobile` is a MAUI Blazor Hybrid template. It hosts Razor components in `BlazorWebView`; targets are conditional by host OS in its project file. It has no MudBlazor package, API client, SQLite store, auth, or product workflows yet.
- `HF.Web` is a Blazor WebAssembly template targeting .NET 10 with MudBlazor `9.*`. Its weather page reads bundled sample JSON. MudBlazor services are registered and providers are placed in the main layout; inspect CSS and assets before changing them.
- `HF.API` still contains a generated weather sample. Neither UI currently consumes its REST endpoints. Do not invent routes or claim integration exists.
- Read focused skills under `.agents/skills/`. Query the configured `mudblazor` MCP for MudBlazor component APIs and examples; match its version to the version NuGet actually resolved.

## Project boundaries

- `HF.Domain`: framework-independent domain concepts, invariants, entity-specific conflict decisions.
- `HF.Application`: use cases and ports; no concrete database or HTTP implementations.
- `HF.Infrastructure`: persistence/external adapters; isolate provider-specific code.
- `HF.API`: HTTP transport, authentication/authorization, validation, mapping, and application use cases.
- `HF.Composition`: dependency registration and host wiring.
- `HF.Mobile`: MAUI UI, BlazorWebView, and platform APIs. Keep reusable domain/application behavior out of components.
- `HF.Web`: browser Blazor WebAssembly consumer. Do not assume MAUI APIs, protected local device secrets, or server privileges.
- Preserve `HF.*` naming and .NET 10 target unless a documented repository-wide change is approved.

## Coding, persistence, and security

- Keep nullable and implicit usings enabled. Use explicit API contracts; do not expose persistence models directly.
- Pass CancellationToken through async boundaries; avoid sync-over-async. Keep tests deterministic.
- Bind typed options and validate startup configuration. Never commit secrets or production credentials.
- Before choosing Turso/libSQL, test exact EF Core/provider versions for query translation, transactions, migrations, generated keys, constraints, concurrency, retries, backups, and restore.
- Version server and local schemas independently; stale clients need explicit compatibility.
- Enforce tenant scope in server authorization and persistence, not only client filters.
- Recheck organization/unit membership for every operation. Device credentials are scoped and revocable, and do not replace user authorization.
- Keep local edits + Outbox atomic, Inbox + server effect atomic, duplicate Push idempotent, Pull page + cursor atomic, and cursor opaque/server-issued.
- Use optimistic concurrency and explicit conflict rules per entity. Never silently apply generic last-write-wins.
- Keep media outside relational rows with separate authorization and resumable transfer.
- Never log student names, contact details, tokens, credentials, or raw payloads. Use synthetic data only.

## Tests

Cover domain rules, cross-tenant denial, Outbox/Inbox atomicity, duplicate Push, cursor ordering/expiry, interrupted Pull, conflicts, local schema upgrades, and offline flows. Avoid live services except opt-in integration tests.
