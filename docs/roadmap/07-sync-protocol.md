# 07 — Bidirectional synchronization protocol

## Objective

Define and implement durable Push/Pull semantics that remain correct through retries, interruption, and intermittent connectivity.

## Dependencies

Phases 04–06.

## Work

- Version the sync envelope and define stable operation IDs, entity IDs, base versions, payload validation, and per-operation outcomes.
- Implement local transactional Outbox and server transactional Inbox. Duplicate Push returns the original durable result and never repeats a domain effect.
- Implement server-issued ordered change feed and opaque scope-bound Pull cursor. Define stable page ordering, page sizing, cursor expiry/reset, tombstones, and full-resync path.
- Apply each Pull page and its next cursor atomically in local SQLite. Ensure partial failure leaves both unchanged.
- Recheck authorization for each Push and Pull. Define scope changes and revocation behavior.
- Specify bounded retry/backoff, cancellation, partial batch results, poison-operation handling, and protocol compatibility for stale clients.

## Acceptance criteria

- Duplicate and reordered delivery tests prove idempotency and ordering.
- Fault-injection tests cover timeouts, app termination, partial batch errors, server transaction failure, Pull apply failure, cursor expiry, and authorization changes.
- No acknowledged local operation is lost; no Pull cursor advances past unapplied data.
- Protocol and API versions are documented and testable across supported client versions.

## Risks and open decisions

Cursor storage/encoding and feed retention must be selected for the validated provider. Do not use timestamps as a substitute for a consistent ordered feed.
