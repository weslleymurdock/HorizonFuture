# HorizonFuture agent instructions

## Repository state

- Early .NET 10 solution under `src/`: `HF.API`, `HF.Application`, `HF.Composition`, `HF.Domain`, `HF.Infrastructure`, `HF.Mobile`, and `HF.Web`.
- `HF.Mobile` is a MAUI Blazor Hybrid template targeting Android and, on supported build hosts, iOS, MacCatalyst, and Windows. It currently has no MudBlazor package, product workflow, local persistence, or API integration.
- `HF.Web` is a Blazor WebAssembly template targeting `net10.0` with MudBlazor `9.*`. Its weather sample reads bundled JSON; it is not integrated with the REST API.
- `HF.API` still has the generated weather sample. Production domain, authentication, persistence, and sync are not implemented.
- Read scoped guidance in `src/AGENTS.md` and `docs/AGENTS.md`. Antigravity skills are in `.agents/skills/`; workspace MCP configuration is `.agents/mcp_config.json`.
- Keep these baseline facts current. Treat roadmap statements as plans, not evidence of implementation.

## Architecture direction

The intended product is an offline-first app for an NGO with multiple school units:

- .NET MAUI clients use local SQLite for normal reads/writes and remain useful without connectivity.
- ASP.NET Core REST API is the authenticated server boundary.
- Turso/libSQL is a candidate central database only after validating current EF Core/provider compatibility, transactions, migrations, generated values, concurrency, and operations. Do not infer compatibility from SQLite branding; record evidence and keep provider code behind abstractions.
- Resolve organization/unit scope from authenticated identity and server-side membership. Never trust client tenant identifiers as authorization.
- Bidirectional Push/Pull sync uses transactional Outbox/Inbox, idempotency, a server-issued ordered change feed/cursor, optimistic concurrency, and explicit entity-specific conflict policies.
- Register and revoke devices; connectivity is intermittent and retries/scheduling must be safe.
- Store media outside relational rows, with separately authorized references and transfer lifecycle.

These are implementation requirements, not claims that they exist today. The ordered plan is in `docs/roadmap/`.

## Repository conventions

- Follow the `HF.*` project structure. Keep Domain framework-independent; Application owns use cases and ports; Infrastructure owns adapters; API owns HTTP and server authorization; Composition wires dependencies; Mobile owns MAUI UI/device APIs; Web is the browser client.
- Current projects target .NET 10 with nullable and implicit usings enabled. Web uses Blazor WebAssembly and MudBlazor 9.*; Mobile hosts Razor components through `BlazorWebView` and does not yet reference MudBlazor. Preserve targets unless a deliberate upgrade is documented.
- Use async I/O and pass cancellation tokens at boundaries. Keep transport DTOs separate from domain and persistence types.
- Prefer focused types, structured logs, typed configuration, and explicit error handling. Keep secrets, student PII, and real records out of source, logs, tests, and docs.
- Do not introduce dependencies from Domain to Application, Infrastructure, API, or UI projects.

## Engineering practices

- Inspect relevant project files, package versions, API contracts, migrations, and tests before edits.
- Add/update tests for behavior changes, especially authorization and sync retries. Report only checks actually run.
- Treat schemas as compatibility contracts for stale offline clients. Plan migrations, rollback/recovery, and expand/migrate/contract where needed.
- Preserve sync invariants: local mutation and Outbox commit atomically; Inbox and server effect commit atomically; retries do not duplicate effects; Pull page and cursor commit atomically; cursors are opaque/server-issued; auth is rechecked; conflicts follow entity-specific policy.
- Record provider evidence, API/protocol changes, conflict policy, and rollout constraints.
- Keep media bytes outside relational storage by default.

## Antigravity and MCP

- Use focused workspace skills in `.agents/skills/` for MAUI Blazor Hybrid, Web MudBlazor, REST API consumption, and MudMCP reference.
- Query the configured `mudblazor` MCP for version-specific component APIs/examples before authoring MudBlazor markup. Compare with the resolved package version. Never send private source excerpts or student data to this documentation service.
- MudMCP is exposed at `http://localhost:8000/mcp`; manage it with `scripts/mudmcp-install.ps1`, `scripts/mudmcp-start.ps1`, `scripts/mudmcp-stop.ps1`, and `scripts/mudmcp-uninstall.ps1`. Scripts build the image from pinned upstream release source, bind to loopback, and preserve cache on stop. Review upstream version/license before upgrades.

## Documentation

- Keep roadmap numbers stable; append new phases and update the index.
- Mark assumptions, decisions, and checks still required. Use `docs/guidelines/` for durable topic guidance.

## Completion report

List exact changed paths, validation performed, and commit hash when requested. Distinguish planned from implemented behavior.
