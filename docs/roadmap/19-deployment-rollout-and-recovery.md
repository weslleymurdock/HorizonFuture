# 19 — Cloud deployment, rollout, and recovery

## Objective
Release the system safely as an independent cloud installation for each NGO, with PostgreSQL as the server database and SQLite only on offline clients.

## Dependencies
Stages 11–18.

## Work
- Provision one Render backend, one managed PostgreSQL database/project, one Infisical project/environment, and one MAUI configuration per NGO installation.
- Keep development, staging, and production resources, credentials, and synthetic/production data separated.
- Configure TLS, least privilege, health checks, secrets, backups, restore, monitoring, retention, and incident response.
- Deploy schema/API changes using backward-compatible expand/migrate/contract sequencing and a defined offline-client compatibility window.
- Pilot with selected units/devices; define onboarding, training, feedback, rollback, and exit criteria.
- Validate quota/capacity and paid-plan transition before production reliance; free tiers are not availability guarantees.
- Define data export/deletion, lost-device handling, credential rotation, support ownership, and recovery exercises.

## Acceptance criteria
- Deployment manual documents service/database/secret provisioning and environment separation.
- PostgreSQL backup restore, migration recovery, credential rotation, and stale-client upgrade are exercised.
- Pilot exit criteria include financial integrity, sync correctness, privacy, support readiness, and operational thresholds.
- Release approval and known limitations are recorded.

## Risks and open decisions
Hosting region, production plans, data residency, operational owner, RPO/RTO, and support obligations require approval. Do not treat free-tier quotas as production capacity.
