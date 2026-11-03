# 10 — Verification and observability

## Objective

Prove isolation, offline correctness, sync reliability, migration safety, and production diagnosability without collecting sensitive payload data.

## Dependencies

Phases 03–09.

## Work

- Build layered domain, application, API authorization, persistence integration, local SQLite migration, contract, and MAUI workflow tests.
- Add property/fault tests for duplicate operations, reorderings, clock skew, cursor paging, interruption, and conflict policy.
- Use synthetic data and include cross-tenant adversarial tests. Keep cloud/database integration tests explicitly opt-in and isolated.
- Add metrics for sync attempts/outcomes, queue age/size, retry counts, cursor resets, conflicts, API latency/errors, and media transfer failures.
- Use correlation, operation, and device identifiers while redacting PII and sync payloads. Define dashboards and alert thresholds.
- Document support diagnostics and safe export procedures that remove sensitive data.

## Acceptance criteria

- Required test suites run in CI or a documented gated pipeline and report results.
- Security and reliability failure paths are reproducibly exercised.
- Operational metrics and alerts identify stuck queues and elevated failure rates without exposing student records.
- Runbooks guide support staff through diagnosis, recovery, and escalation.

## Risks and open decisions

Metrics retention, service-level objectives, support ownership, and observability provider require operational agreement.
