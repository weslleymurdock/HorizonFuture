# Synchronization invariants

These are requirements for the planned offline sync implementation. They are not claims about current code.

## Local writes and Push

- A user-visible local mutation and its Outbox operation are written in the same SQLite transaction. Either both commit or neither does.
- Every operation has a stable client-generated idempotency identifier and enough version/base-version metadata to support server validation.
- The server Inbox records the operation identifier and result in the same transaction as the accepted domain effect. Repeated delivery returns the recorded outcome and does not duplicate effects.
- A failed or timed-out request may be retried safely. The client retains the operation until it receives a durable server outcome.

## Pull and cursor

- The server issues an opaque cursor for a specific authorized scope and feed position. It represents a stable, ordered continuation point; clients never calculate it from timestamps or entity IDs.
- Change ordering and pagination must not skip changes when new writes occur during a multi-page Pull.
- The client applies a Pull page and saves its returned cursor atomically. If application fails, neither the changes nor the cursor advance.
- Cursor expiry, reset, authorization-scope changes, and full resynchronization have explicit protocol behavior.
- Authorization is checked on every page and Push operation, including after membership or device revocation.

## Concurrency and conflict policy

- Use optimistic concurrency to detect stale writes. Do not silently overwrite server or local changes.
- Specify policy per entity and field semantics. Examples to evaluate include safe merge of independent fields, additive handling of append-only events, server rejection requiring refresh, and user-mediated resolution for sensitive records.
- Preserve enough conflict context for safe retry or user review. Define when a rejected operation may be edited, superseded, or discarded and how that decision is audited.

## Reliability and privacy

- The sync loop tolerates duplicate delivery, interruption, app termination, clock skew, no connectivity, and partial server failure.
- Backoff is bounded and observable, and background scheduling is best-effort; correctness cannot depend on a scheduler running at an exact time.
- Logs and telemetry carry operation/device/correlation identifiers but exclude student PII and payload contents.
