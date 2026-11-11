# Privacy, security, evidence, and offline operation

## LGPD and minimization

Data must be collected only for a documented operational purpose. For each
category, the next stage must confirm purpose, legal basis with a qualified
professional, access, retention, deletion/anonymization, export, sharing,
data-subject response, backup, and incidents. Consent is not presumed to be an
appropriate basis for every operation.

Data about children/adolescents, families, health, vulnerability, financial
documents, and contacts requires stricter classification and access. Free-form
notes risk excessive collection; the MVP should prefer necessary fields,
validation rules, and scope. Audit should record access and changes without
duplicating sensitive content in logs.

Documents and media remain outside PostgreSQL, with metadata, owner,
classification, hash/integrity, version, retention, temporary access
authorization, scanning, limits, and interruption behavior. This is a
discovery requirement, not an implementation at this stage.

## Threats and controls to validate

| Threat | Consequence | Required control |
|---|---|---|
| user sees unauthorized unit/project | exposure or fraud | server authorization by user, role, and scope |
| lost device | local data and credentials exposed | registration/revocation, secure storage, and expiry |
| token expires offline | operation beyond current authority | limited window and queue subject to revalidation |
| duplicate Push | duplicate payment/effect | Inbox, idempotency, and operation key |
| financial conflict | incorrect balance/accountability | optimistic concurrency and review, never generic last-write-wins |
| public media through predictable URL | document leak | authorized reference and temporary access |
| log contains PII/token | enlarged incident | minimized structured logs and review |
| duplicate statement import | incorrect balance/reconciliation | file/line identification and auditable closing |
| executor approves own action | fraud or unreviewed error | maker-checker and explicit delegation |

## Classification of offline operations

| Class | Examples | Synchronization condition |
|---|---|---|
| 1. Fully offline | previously authorized data query; activity draft | later update |
| 2. Offline with later validation | attendance, service delivery, document draft | server revalidates version, scope, and conflict |
| 3. Offline, completion dependent | expense proposal, preliminary allocation, accountability preparation | not presented as approved until return |
| 4. Connectivity required | granting authority, final approval, financial posting, reconciliation closing | block completion and explain why |
| 5. Prohibited offline | access change, revocation, high-risk or fraud-prone action | do not create an equivalent local effect |

The product must show local state, last update, conflicts, rejections,
possibly stale balance, and required action. It must not hide pending work as
success. Push must be repeatable without duplication; Pull must confirm page
and cursor together; permissions and conflicts are rechecked on the server.

## Installation guidelines

Each NGO receives an independent backend, PostgreSQL, secrets, configuration,
and application URL. Units and departments are internal scopes, not tenants.
PostgreSQL is the only server database; SQLite is exclusive to MAUI local
persistence. Outbox/Inbox architecture, ordered cursor, optimistic
concurrency, and conflict rules will be designed only in stages 14–17, after
domain and backend acceptance.
