# Documentation agent guidelines

These instructions apply to `docs/`. Read the root `AGENTS.md` first.

- Describe the repository as it exists and label proposed architecture, assumptions, open decisions, and validation evidence clearly.
- Roadmap phases are implementation plans. Do not write them in the past tense or imply their acceptance criteria have passed.
- Keep phase files numbered and stable. Preserve existing numbering; append phases if the sequence grows and update the index.
- Each phase should state objective, dependencies, work items, acceptance criteria, risks/open decisions, and evidence to record.
- Prefer concrete checks (tests, migrations, deployment exercises, measured compatibility results) over vague completion language.
- Treat student data as sensitive. Use synthetic examples and avoid personal data in documentation.
- Keep architecture decisions traceable. Record provider compatibility and material changes to data, API, sync, conflict, and rollout contracts.
- The planned deployment is one backend and PostgreSQL database per NGO, with separate secrets/configuration and a MAUI backend URL per installation. Do not add cross-NGO tenant partitioning to ordinary application data paths; any future aggregation service is optional and separately authorized.
- Preserve PostgreSQL as the only server database and SQLite exclusively as the MAUI local store. Preserve the NGO social/funding/financial domain and its audit/restriction invariants.
- The 19-stage roadmap is domain-first: stages 01–10 define and accept the backend domain; 11–13 establish API, PostgreSQL validation, and server persistence; MAUI starts at stage 14. Do not schedule client implementation before stages 01–13 are accepted.
- Preserve offline-first MAUI SQLite and the planned transactional Outbox/Inbox, idempotent Push, ordered server cursor, optimistic concurrency, and entity-specific conflict rules when updating architecture or deployment docs.
- Keep links relative and verify that they resolve after changes.
