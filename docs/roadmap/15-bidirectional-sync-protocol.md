# 15 — Bidirectional synchronization protocol

## Objective
Implement durable Push/Pull synchronization between local SQLite and the cloud PostgreSQL backend.

## Dependencies
Stages 11–14.

## Work
- Version sync envelopes; define operation IDs, entity IDs, base versions, payload validation, per-operation outcomes, and compatibility windows.
- Implement local transactional Outbox and server transactional Inbox. Duplicate Push returns the durable original outcome and never repeats a financial or domain effect.
- Implement server-issued ordered change feed and opaque scope-bound Pull cursor, including stable ordering, expiry/reset, tombstones, and full-resync behavior.
- Apply each Pull page and its cursor atomically in local SQLite.
- Recheck user, device, unit, project, source, and financial-account authorization on every Push/Pull.
- Define bounded retries/backoff, cancellation, partial batch outcomes, poison operations, and stale-client behavior.
- Prevent replay or duplicate posting of receipts, transfers, expenses, approvals, and reconciliation actions.

## Acceptance criteria
- Duplicate/reordered delivery tests prove idempotency and stable ordering.
- Fault injection covers timeout, termination, partial batch, transaction failure, Pull apply failure, cursor expiry, and authorization revocation.
- No acknowledged operation is lost and no cursor advances past unapplied data.
- Protocol contract is versioned and tested across supported client versions.

## Risks and open decisions
Do not use timestamps as the ordering primitive. Financial operations require stronger idempotency and audit semantics than ordinary profile edits.
