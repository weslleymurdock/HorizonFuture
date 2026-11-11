# 12 — PostgreSQL provider and cloud compatibility gate

## Objective
Make PostgreSQL the only server relational database and validate the exact EF Core/Npgsql/cloud deployment behavior before final persistence implementation.

## Dependencies
Stages 01–11.

## Work
- Remove SQLite as a server database option from the target architecture. SQLite remains exclusively a local MAUI database for offline operation.
- Validate supported .NET 10, EF Core, Npgsql versions and PostgreSQL behavior against a managed cloud instance.
- Exercise transactions for financial posting, budget allocation, Inbox idempotency, change-feed records, and reconciliation updates.
- Validate decimal/numeric precision, constraints, indexes, generated IDs, concurrency tokens, row locking/isolation, query plans, paging, connection pooling, transient retries, and migrations.
- Validate PostgreSQL backup/restore, TLS, least-privilege roles, secrets injection, connection limits, and hosting-region requirements.
- Record provider comparison, quota/cost limits, data residency, backup guarantees, and a paid-tier migration plan; free tiers are evaluation only.
- Keep provider-specific code in Infrastructure and avoid leaking Npgsql types into Domain/Application.

## Acceptance criteria
- Reproducible compatibility test suite passes against the selected managed PostgreSQL service.
- Monetary precision and transactional invariants are proven with integration tests.
- The server architecture and deployment documentation explicitly state PostgreSQL only; no server SQLite fallback remains.
- Provider decision includes exact versions, operational limits, backup/restore evidence, and production caveats.

## Risks and open decisions
A free-tier quota is not a production availability guarantee. Confirm data protection, backup retention, region, and financial data recovery requirements before production.
