# Security and privacy guidance

Student and family records require careful handling. This guidance applies to planned implementation work and does not assert that controls already exist.

- Minimize collected fields and local retention. Document purpose, access, retention, deletion, and backup behavior for each sensitive data category.
- Enforce least privilege using authenticated user identity, organization membership, school-unit assignment, and role/capability checks. Recheck server authorization for every REST and sync operation.
- Never trust tenant scope, role, device identity, or record ownership supplied only by the client.
- Protect API traffic with TLS. Store device credentials and local database encryption keys using platform secure storage; evaluate encrypted SQLite and key recovery/rotation needs before release.
- Protect central credentials in a secret manager or deployment secret store. Never commit secrets or put credentials in logs, query strings, crash reports, or client binaries.
- Keep logs, analytics, crash reports, and test fixtures free of student names, contacts, credentials, tokens, and raw sync payloads. Use synthetic data.
- Define device registration, user/device association, revocation, lost-device response, and offline token-expiry behavior. Offline access must be bounded and must not grant scope beyond the last authorized membership policy.
- Keep media outside relational storage and require authorized, short-lived transfer access. Define scanning, content limits, retention, deletion, and interrupted-transfer behavior.
- Plan audit events for sensitive access and changes without duplicating sensitive record contents. Restrict and retain audit data deliberately.
- Before production, review applicable privacy and child-protection obligations with the NGO's responsible stakeholders and document operational controls.
