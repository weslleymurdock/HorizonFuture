# Documentation agent guidelines

These instructions apply to `docs/`. Read the root `AGENTS.md` first.

- Describe the repository as it exists and label proposed architecture, assumptions, open decisions, and validation evidence clearly.
- Roadmap phases are implementation plans. Do not write them in the past tense or imply their acceptance criteria have passed.
- Keep phase files numbered and stable. Preserve existing numbering; append phases if the sequence grows and update the index.
- Each phase should state objective, dependencies, work items, acceptance criteria, risks/open decisions, and evidence to record.
- Prefer concrete checks (tests, migrations, deployment exercises, measured compatibility results) over vague completion language.
- Treat student data as sensitive. Use synthetic examples and avoid personal data in documentation.
- Keep architecture decisions traceable. Record provider compatibility and material changes to data, API, sync, conflict, and rollout contracts.
- Keep links relative and verify that they resolve after changes.
