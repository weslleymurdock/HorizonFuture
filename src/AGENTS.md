# Source code agent guidelines

These instructions apply to all projects under `src/`. Read the root `AGENTS.md` first.

## Project boundaries

- Keep `HF.Domain` framework-independent and responsible for domain concepts, invariants, and entity-specific conflict decisions.
- Keep `HF.Application` use cases and ports/interfaces. It should not depend on concrete database or HTTP implementations.
- Keep `HF.Infrastructure` adapters for persistence, external services, device identity support, and sync infrastructure. Isolate provider-specific code behind explicit adapters.
- Keep `HF.API` limited to HTTP transport, authentication/authorization enforcement, request validation, mapping, and composition with application use cases. REST endpoints must not bypass application authorization or persistence rules.
- Use `HF.Composition` for dependency registration and host wiring shared by executable projects. Add the MAUI client as a distinct project when its scope is implemented; do not put UI code in server projects.
- Preserve the existing `HF.*` naming and .NET 10 target unless a repository-wide upgrade is explicitly planned.

## Coding conventions

- Enable nullable reference types and implicit usings consistently. Prefer explicit public API contracts and avoid exposing persistence entities directly over HTTP.
- Pass `CancellationToken` through asynchronous API, persistence, and network boundaries. Do not use sync-over-async.
- Keep domain time and identifiers deterministic in tests by injecting abstractions where behavior depends on them.
- Bind configuration to typed options and validate it during startup. Never commit secrets or production endpoints with credentials.
- Use structured logs with correlation/operation identifiers. Do not log student names, contact details, credentials, access tokens, or sensitive payloads.

## Persistence and schema

- Keep API contracts, domain models, and persistence models separate where their change lifecycles differ.
- Before selecting a central Turso/libSQL provider, record a compatibility spike for the exact EF Core and provider versions in use. Verify supported query translation, transactions, migrations, generated keys, unique constraints, optimistic concurrency, pooling/retry behavior, and backup/restore procedures. If validation fails, propose and document an alternative; do not silently weaken invariants.
- Define schema ownership and migration order. Server and local SQLite schemas evolve independently, with explicit compatibility/version handling for offline clients.
- Tenant keys, school-unit scope, uniqueness, and foreign-key rules must be enforced in server-side persistence and authorization paths, not only by client filters.
- Never run destructive schema operations automatically against production or user data.

## API, authorization, and sync

- Enforce authentication and organization/unit membership on every resource operation. Resolve allowed scope from the authenticated principal and server-side membership. Treat client-supplied scope only as a selection hint that must be checked.
- Device credentials must be unique, scoped, revocable, stored securely on the device, and never treated as a replacement for user authorization.
- Make Push idempotent using stable operation identifiers and an Inbox/deduplication record committed with the domain effect. Persist local changes and Outbox entries in one local transaction.
- Pull changes from a server-issued, ordered change feed. Cursors are opaque and scope-bound; clients must not invent, compare, or advance them. Apply a Pull page and its next cursor atomically in local storage.
- Use optimistic concurrency with explicit per-entity conflict behavior. Do not resolve all conflicts using last-write-wins. Preserve unresolved conflicts for user or domain review.
- Keep media bytes in object/file storage, not relational rows. Authorize metadata and transfer separately, and support interrupted transfer.

## Tests

- Cover domain rules, API authorization (including cross-organization and cross-unit denial), tenant isolation, transactional Outbox/Inbox behavior, duplicate Push, cursor ordering/expiry, interrupted Pull, concurrency and conflict policies, schema upgrade from prior local versions, and offline operation.
- Use synthetic data only. Never use real student data in fixtures or snapshots.
- Keep tests deterministic; avoid dependence on a live Turso account or network except in explicitly opt-in integration suites.
