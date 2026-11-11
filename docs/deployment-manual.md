# Per-NGO deployment manual

## Deployment model and current status

The planned deployment is one independent installation per NGO. Each installation has its own Render backend service, managed PostgreSQL project/database, Infisical project and environment secrets, and MAUI app configuration pointing at that installation's backend URL. Normal application operation and synchronization use only those installation resources. Per-installation application tables do not need tenant IDs or tenant-isolation filters. User, role, record ownership, and school-unit authorization still apply inside that NGO's backend.

A separately authorized cross-installation reporting/aggregation service is an optional future component. It must be independently provisioned, receive explicit authorization from each participating NGO, and must not be required for ordinary app operation or sync.

This is a target deployment model, not a claim that production infrastructure or sync is ready. The repository has backend foundations and UI scaffolds; the expanded NGO financial domain, production deployment, complete offline client, and synchronization remain roadmap work.

## Provider selection and free-tier evidence

Checked on **2026-11-03** against official provider pages:

| Provider | Free PostgreSQL storage | Relevant limits |
|---|---:|---|
| Neon | **1 GB per project** | Free plan includes 100 CU-hours and 5 GB public transfer per project/month; compute scales to zero after 5 minutes idle; usage limits suspend compute until the next billing period; storage writes fail above 1 GB per project or 20 GB combined; no inactivity pause or expiry is stated. |
| Supabase | 500 MB per project | Free projects pause after one week of low activity; shared Nano compute (up to 0.5 GB RAM); no automatic backups. |

Neon is selected because its currently documented free PostgreSQL storage per project (1 GB) is greater than Supabase's (500 MB). This quota comparison does not establish production suitability. Neon Free allowances are development/evaluation quotas: compute and transfer are bounded, and service may be suspended at quota; evaluate NGO availability, backup, support, data residency, privacy, and expected workload before production. Supabase's inactivity pause also makes it a weaker fit for an always-available pilot. Do not select Render Postgres as the app database: its free instance has 1 GB but expires 30 days after creation, has no backups, and only one may be active per workspace.

Official sources (checked 2026-11-03):

- Neon pricing and limits: <https://neon.com/pricing>
- Supabase pricing: <https://supabase.com/pricing>
- Supabase free-project behavior: <https://supabase.com/kb/faqs/platform/free-postgres-for-side-projects>
- Render free instance limits: <https://render.com/docs/free>

Recheck provider pages before provisioning because quotas and terms can change. These free plans are not production recommendations or capacity guarantees.

## Provisioning sequence

Repeat this sequence for each NGO, and keep development, staging, and production resources and credentials separate. Use explicit labels such as `horizon-<ngo>-dev`, `horizon-<ngo>-staging`, and `horizon-<ngo>-prod`; do not place multiple NGOs' operational data in one installation.

### 1. Create Render hosting

1. Create or use the NGO/operator-owned Render account/workspace and enable MFA for administrators.
2. Connect the repository and create a separate Web Service for the backend for each environment. The current API is a .NET template; configure Render to build/run its supported Docker deployment after the backend container/deployment configuration is implemented. Do not invent build commands until that implementation exists.
3. Use an environment-specific service name and region chosen to meet the NGO's data-location and latency requirements.
4. Configure health checks, deploy notifications, TLS/custom domain if required, and a restricted operator team. Record the generated HTTPS backend URL.
5. Treat Render Free as evaluation only: a free web service sleeps after 15 minutes without inbound traffic and takes about one minute to start; it has an ephemeral filesystem, can restart, and may be suspended by workspace monthly usage limits. Never store application data in its local filesystem. Plan a paid service for production availability.

### 2. Create the managed PostgreSQL database in Neon

1. Create an NGO-owned Neon account/workspace and a distinct project for each environment. Set region to the approved data location.
2. Create the production database and a separate least-privilege application role; keep operator/admin access separate from runtime credentials.
3. Save the connection string as a secret directly into the matching Infisical environment (step 3). Use TLS/SSL connection settings required by Neon. Do not place it in Git, app configuration files, MAUI binaries, or logs.
4. Establish tested backup/export and restore procedures before production. Neon Free's 1 GB, 100 CU-hour, and 5 GB transfer limits are strict operational limits; over-quota compute/transfer suspends compute for the remainder of the billing period and storage increases fail above quota. Upgrade to an appropriate paid plan before production use if the NGO's availability or workload needs exceed those constraints.
5. Keep migration credentials separate from the runtime role; apply versioned migrations through a controlled deployment process once migrations exist.

### 3. Create Infisical secret/configuration environments

1. Create an Infisical account/workspace and a project for the NGO installation. Add separate `dev`, `staging`, and `prod` environments.
2. Create distinct secret values in each environment. At minimum, plan for `ConnectionStrings__PostgresConnection` (or fallback `ConnectionStrings__DefaultConnection`), auth/signing and token-validation settings once implemented, and any external integration credentials. The MAUI API base URL is configuration, not a secret.
3. Give each Render environment only access to the matching Infisical environment, using a scoped machine identity/service token. Do not reuse a production token in development or across NGOs.
4. Configure Render environment variables or the implemented secret-fetch mechanism using Infisical's documented integration. Ensure secrets are masked and excluded from deploy logs. Rotate credentials when operators change or access is suspected.
5. Document ownership and recovery contact for the Infisical project. Never commit `.env` files containing real values.

### 4. Configure and distribute the MAUI app

1. Build an environment-specific app configuration containing that installation's HTTPS backend base URL. Do not include database credentials, Infisical credentials, signing keys, or privileged API keys in the client.
2. Confirm build/release identity and display name identify the NGO/environment where needed to avoid operators installing the wrong app configuration.
3. Distribute the app only to the intended NGO cohort. Keep dev, staging, and production builds/configuration distinct; verify the URL before release.
4. The MAUI app uses local SQLite exclusively for normal reads/writes while offline. PostgreSQL is the only server relational database; the backend must not use SQLite. The server URL only becomes necessary for authenticated sync and other server-backed operations. Preserve transactional Outbox/Inbox, idempotent Push, ordered server-issued Pull cursor, atomic local page+cursor commits, optimistic concurrency, and entity-specific conflict handling.
5. Record installation URL, environment, app version, schema/API compatibility window, database migration version, and responsible operator without recording secrets or personal records.

## Environment separation and operations

- One NGO production installation uses one Render backend, one Neon production project/database, and one Infisical project with isolated environment values. Development and staging use separate service/database environments and synthetic data.
- Keep app base URL, database, service credentials, signing keys, and operator access environment-specific. Production data must not be copied into development or staging.
- Apply backward-compatible database/API migrations before releasing clients that depend on them. Test backup restoration, credential rotation, and offline client upgrade behavior.
- Set capacity and alerts below free-tier ceilings. Define a migration to paid service/database plans before the NGO depends on uptime or approaches quota. Render's free Postgres is not the chosen provider and is not suitable for persistent app data due to expiry and backup limits.
- Keep media bytes outside relational rows, with separately authorized references and a selected storage lifecycle once that component is designed.
- Any future aggregation/reporting component has its own authorization model, secrets, deployment, audit trail, and consent/contract from each source NGO. It reads only approved exports or APIs and is never on the regular sync path.

## Pre-production checklist

- [ ] Correct NGO and environment labels appear in Render, Neon, Infisical, and the distributed app.
- [ ] API health checks and TLS work at the intended public URL.
- [ ] Runtime database role has only required permissions; migration and operator roles are separate.
- [ ] Secret values are environment-scoped, masked, and not present in source, client binaries, or logs.
- [ ] Provider quota, backup/restore, monitoring, data location, privacy requirements, recovery owner, and paid-plan decision are documented.
- [ ] Local SQLite/offline operation and the existing planned Push/Pull invariants are retained; deployment does not imply they are implemented.
- [ ] NGO-specific user, role, device, and school-unit authorization is reviewed and tested.

## Provider evidence detail

The official Neon pricing page lists Free as 1 GB storage per project, 100 CU-hours per project monthly, and 5 GB transfer per project. Its FAQ says compute scales to zero after five idle minutes; using the monthly compute/transfer limits suspends compute until the next billing period, and storage-increasing operations fail above 1 GB per project or 20 GB across projects. It says Free projects do not pause for inactivity. The official Supabase pricing page lists 500 MB database size per project; its official FAQ says Free projects pause after a week of low activity. Render's official free-tier documentation says Free Postgres is 1 GB, expires after 30 days, has a 14-day upgrade grace period before deletion, and has no backups; Free web services sleep after 15 minutes without traffic.
