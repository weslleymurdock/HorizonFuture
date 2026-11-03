# 11 — Deployment, rollout, and recovery

## Objective

Release incrementally with tested schema/API compatibility, protected data, and a practical recovery path.

## Dependencies

Phases 03–10.

## Work

- Define environments, secret management, network controls, TLS, least-privilege access, backups, restore tests, retention, and incident response.
- Deploy API and database changes using backward-compatible expand/migrate/contract sequencing. Keep old clients able to function during the agreed support window.
- Establish staged pilot across selected school units and devices, with explicit enrollment, training, feedback, and rollback criteria.
- Verify monitoring, alert routing, capacity assumptions, provider quotas/limits, cost controls, backup recovery, and credential/device revocation before broad release.
- Define client distribution, version support, forced-upgrade criteria, migration failure recovery, data export/deletion, and support responsibilities.
- Conduct security/privacy review and tabletop exercises for lost device, compromised credentials, tenant leakage, and provider outage.

## Acceptance criteria

- A documented deployment runbook and tested recovery/rollback procedure exist.
- Backup restore, migration from supported prior schemas, credential rotation, and staged client upgrade have been exercised in a non-production environment.
- Pilot exit criteria are met, including sync correctness, support readiness, privacy controls, and operational thresholds.
- Release approval and known limitations are recorded; no assumption of unlimited free-tier capacity is used for sizing.

## Risks and open decisions

Hosting region, operational ownership, provider plan, service objectives, rollout cohort, and applicable privacy obligations remain decisions until validated.
