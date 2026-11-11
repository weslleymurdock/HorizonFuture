# 11 — REST API, identity, permissions, and device trust

## Objective
Expose approved domain use cases through a secure, versioned API for one NGO installation.

## Dependencies
Stages 01–10.

## Work
- Define REST resources and explicit request/response contracts for stakeholders, service delivery, projects, funding, budgets, financial operations, reconciliation, accountability, and reports.
- Keep transport DTOs separate from Domain and persistence entities; define validation, pagination, filtering, idempotency, error semantics, and API compatibility.
- Define authentication, token issuance/refresh/revocation, invitation and account lifecycle, roles/capabilities, and approval authority.
- Enforce permissions for unit, project, funding source, financial account, and sensitive stakeholder records on every request.
- Define device registration, credential rotation/revocation, lost-device response, and secure local credential storage requirements.
- Define approval/authorization checks for financial posting, budget amendments, transfers, refunds, and report submission.
- Apply rate limits, request-size controls, audit events, safe logging, and privacy protections.
- Do not expose arbitrary EF entities or permit client-submitted balances/approval states to be trusted.

## Acceptance criteria
- Versioned API contract and permission matrix cover all MVP bounded contexts.
- Tests prove unauthenticated, unauthorized, cross-unit, cross-project, restricted-source, and revoked-device access is denied.
- Financial state transitions are enforced server-side.
- No secrets, financial payloads, or personal data are written to logs.

## Risks and open decisions
Identity provider, MFA requirements, approval delegation, external accountant access, and device/offline session policy require stakeholder/security review.
