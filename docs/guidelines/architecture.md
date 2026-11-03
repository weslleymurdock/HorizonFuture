# Architecture and data boundaries

## Planned system

The current repository has an ASP.NET Core API project and separate Domain, Application, Infrastructure, and Composition projects. The MAUI client and all production capabilities described here are planned, not implemented.

The target is an offline-first .NET MAUI client with local SQLite, an ASP.NET Core REST service, and a central relational store. Domain rules belong in Domain; use cases and ports in Application; concrete storage/network adapters in Infrastructure; HTTP transport and authorization enforcement in API; host wiring in Composition; UI and device integration in a separate MAUI application.

## Database provider gate

Turso/libSQL is a candidate central database, not an assumed implementation choice. Before adopting it:

1. Select exact EF Core and provider versions compatible with the repository's .NET target.
2. Prove required LINQ/query translation, transactions, migrations, generated values, uniqueness/foreign keys, concurrency tokens, retry behavior, and connection lifecycle.
3. Verify backup/restore, deployment networking, operational limits, and data residency/retention requirements for the NGO.
4. Keep the experiment reproducible and record limitations and the decision.
5. If an invariant cannot be preserved, evaluate another provider without coupling domain/application layers to Turso.

The client SQLite schema and central schema are separately versioned. Offline clients may miss several server releases, so API and sync payload changes require explicit compatibility policy and migrations.

## Tenant and school-unit boundaries

Derive organization and unit access from authenticated identity plus server-side memberships and roles. Any organization or school-unit identifier in a request is untrusted input and must be checked against those memberships. Decide whether each entity belongs to an organization, a unit, or both based on the domain model. Enforce scope in application authorization and persistence query/write paths, and test cross-tenant reads and writes.

Media is stored outside relational rows. Relational records carry only authorized metadata and stable references. Transfer tokens, access paths, retention, deletion, and retry behavior need independent design.

## Decision records

Document material choices such as provider selection, identity model, tenant key strategy, stable identifiers, cursor format, payload evolution, and media storage. Include alternatives considered, evidence, consequences, and migration implications.
