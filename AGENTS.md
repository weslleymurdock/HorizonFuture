# HorizonFuture agent instructions

## Repository state and scope

- This repository is an early .NET 10 solution. Current tracked code is under `src/`: `HF.API`, `HF.Application`, `HF.Composition`, `HF.Domain`, and `HF.Infrastructure`. The API still contains the generated weather forecast sample. No MAUI client, school/organization domain, authentication, persistence, sync protocol, or production data provider is implemented yet.
- Treat roadmap documents as planned work, not evidence of implemented behavior. Update them only when repository changes actually complete a milestone.
- Read scoped instructions in `src/AGENTS.md` and `docs/AGENTS.md` when working in those paths.
- Keep changes focused. Do not add packages, services, or infrastructure without explaining their purpose and compatibility.

## Product and architecture direction

The intended product is an offline-first application for an NGO with multiple school units. The target architecture is:

- .NET MAUI clients using local SQLite as the operational store and remaining useful without connectivity.
- An ASP.NET Core REST API as the authenticated boundary for clients.
- A central Turso/libSQL database only if a small compatibility spike validates the current EF Core version, provider support, transactions, migrations, generated values, concurrency, and operational requirements. Do not assume an EF Core provider is compatible because it advertises SQLite support. Record the decision and evidence; keep persistence behind abstractions so an alternative provider can be selected.
- Organization and school-unit scoping based on the actual domain and user membership. Derive scope from authenticated server-side identity and membership, never trust a tenant identifier supplied by a client as authorization.
- Bidirectional Push/Pull synchronization with durable transactional Outbox and Inbox processing, idempotency, a server-issued ordered change feed/cursor, optimistic concurrency, and explicit conflict policy per entity.
- Device registration and revocable device authentication. Connectivity is intermittent; retry and scheduling must be safe and bounded.
- Media and large binary content stored outside relational rows, with authorized references and upload/download lifecycle metadata.

These are design constraints for implementation, not claims that the current code already provides these capabilities. The implementation order is in `docs/roadmap/`.

## Repository conventions

- Follow existing project names and directory layout under `src/`. Keep domain rules independent of ASP.NET Core, MAUI, SQLite, and provider-specific APIs.
- Current project files target `net10.0`, enable nullable reference types, and implicit usings. Preserve those settings consistently unless a deliberate, documented change is needed.
- Use asynchronous I/O APIs with cancellation tokens at boundaries. Validate inputs at application/API boundaries and keep transport DTOs separate from domain models.
- Use explicit names and small cohesive types. Prefer structured logging and configuration binding over ad-hoc console output or embedded secrets.
- Keep secrets, production credentials, real student data, and personal information out of source, tests, fixtures, logs, and documentation.
- Do not introduce a dependency direction from Domain to Application, Infrastructure, API, or MAUI. Composition is responsible for wiring implementations.

## Required engineering practices

- Before changing code, inspect the relevant projects, package versions, existing migrations, API contracts, and tests. Keep this guidance aligned with what is actually present.
- Add or update tests for behavior changes, including authorization boundaries and failure/retry paths for sync. Do not claim a feature is tested unless the corresponding test exists and was run.
- Treat schema changes as compatibility work for both server and potentially stale offline clients. Use expand/migrate/contract where needed; define upgrade, rollback, and recovery behavior before destructive changes.
- Preserve sync invariants: local user changes and their Outbox records commit atomically; retries do not duplicate effects; cursor advancement is committed atomically with applied Pull changes; a cursor is opaque to clients and issued by the server; authorization is rechecked for every Push and Pull; conflicts are surfaced according to an entity-specific policy.
- Document meaningful architecture decisions, provider compatibility evidence, API contract changes, conflict policies, and rollout constraints.
- Never place media bytes in relational database rows by default. Keep media handling separately authorized and resilient to interrupted transfer.

## Documentation changes

- Keep roadmap phase numbers ordered and stable. Add a new numbered phase instead of renumbering existing files; update the roadmap index when phases change.
- Write acceptance criteria that can be verified in the repository or a deployment environment. Label decisions and compatibility checks that remain open.
- Use `docs/guidelines/` for durable topic-specific guidance and link it from the relevant AGENTS.md.

## Completion report

When delivering work, list exact changed paths, summarize validation performed, and report the commit hash when a commit was requested. Distinguish documentation planning from implemented and validated behavior.
