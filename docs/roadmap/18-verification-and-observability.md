# 18 — Verification, security, and observability

## Objective
Prove domain correctness, financial integrity, authorization, offline synchronization, and operational diagnosability.

## Dependencies
Stages 01–17.

## Work
- Build domain/application tests for financial invariants, allocations, restrictions, budgets, lifecycle transitions, project outcomes, and stakeholder privacy.
- Add PostgreSQL integration tests for transaction atomicity, numeric precision, migrations, concurrency, constraints, backup/restore, and query performance.
- Add API authorization, contract, device, and rate-limit tests.
- Add SQLite migration/offline workflow tests and sync property/fault-injection tests.
- Use synthetic data only; include adversarial cross-unit/project/source/account authorization scenarios within one NGO installation.
- Add metrics for API latency/errors, sync attempts/outcomes, queue age, retries, cursor resets, conflicts, financial posting/reversal, reconciliation, and media failures.
- Redact PII, credentials, account numbers, and payloads from logs; define safe support diagnostics, dashboards, alerts, and runbooks.
- Validate report totals against source records and export reproducibility.

## Acceptance criteria
- Required test suites run in CI or a documented gated pipeline.
- Financial and sync invariants have reproducible automated evidence.
- Security tests prove least privilege and sensitive-data isolation.
- Alerts identify stuck sync, reconciliation failures, suspicious posting patterns, and service degradation without leaking data.

## Risks and open decisions
Define SLOs, support ownership, metrics retention, and audit retention with NGO operators.
