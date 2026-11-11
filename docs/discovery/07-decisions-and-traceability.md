# Open decisions, traceability, and acceptance

## Open decision register

| ID | Subject and context | Alternatives and consequences | Next action / owner | Evidence | Status |
|---|---|---|---|---|---|
| ADR-D01 | Reusable identity versus role-specific tables | reuse reduces duplication; separate tables ease transition but duplicate history | validate with organization and model in stage 02 / product + NGO | cases of one person in multiple roles | open |
| ADR-D02 | Social scope: minimal operation versus broad records | minimum reduces risk; broad increases usefulness and exposure | define cases and privacy review / leadership + DPO | purposes and required fields | open |
| ADR-D03 | Double-entry now or auditable operational subledger | double-entry increases rigor and scope; subledger accelerates MVP and requires export/review | choose initial product boundary through internal product/technical analysis; review with accountant after stable release / product + finance | report samples and integration | provisional; external review post-release |
| ADR-D04 | Balance and carry-forward rules | global rule is simple; per-instrument rule is faithful | implement explicit testable policies in stages 06–09; verify with pilot finance staff after stable release | three real anonymized synthetic instruments | blocked by validation |
| ADR-D05 | Approval limits and delegations | fixed is predictable; configurable serves NGO but requires governance | provide configurable policy model in later stage; pilot defines real thresholds after stable release | approved authority matrix | open |
| ADR-D06 | Identity/deduplication/merge criteria | automatic is fast; human review reduces false positives | define signals, blocks, and audit / admin + privacy | test cases and error policy | open |
| ADR-D07 | Offline data and authority window | more offline time helps field work; increases stale-permission risk | security define window and revocation / technical + NGO | loss and revocation scenarios | open |
| ADR-D08 | Statement and accountability formats | one format reduces MVP; adapters serve funders | prioritize two formats after samples / finance | authorized files and layouts | open |
| ADR-D09 | Retention, export, and deletion | long retention helps audit; increases exposure | implement privacy-safe defaults; qualified review after stable release and before broad rollout | applicable documentary policy | blocked by validation |
| ADR-D10 | Bank, gateway, and signature integrations | manual reduces dependencies; integration reduces work and creates vendor risk | assess only after MVP / leadership + technical | cost, security, and contract | deferred |

## Traceability matrix

| Objective | Personas | Workflows | Requirements | Concepts/controls | Roadmap |
|---|---|---|---|---|---|
| Operate services safely | team, participant | person/service | FR-02/03/13, NFR-01/02 | identity, participation, purpose, access | 02–03 |
| Execute and demonstrate projects | coordinator, funder | project/impact | FR-04/16, NFR-03 | activity, indicator, evidence, version | 04 |
| Fund with clear obligations | fundraising, leadership | funding | FR-05/06/16/17 | opportunity, application, source, instrument, installment | 05 |
| Control resources | finance, leadership | budget/operation | FR-07/08/09/10, NFR-04 | allocation, budget item, commitment, movement, maker-checker | 06–08 |
| Explain balances and use | reconciler, auditor | reconciliation | FR-11/12/15, NFR-03/04 | statement, matching, snapshot, audit | 09 |
| Work safely in the field | educator, social team | offline service | FR-14, NFR-05/10 | future Outbox/Inbox, cursor, conflict, revocation | 14–17 |
| Integrate and accept the domain | all | all | FR-01/13, NFR-01/03/09 | contexts, invariants, authorization | 10–13 |

## Stage 01 acceptance checklist

| Criterion | Evidence | Status | Remaining action |
|---|---|---|---|
| Discovery glossary, actors, workflows, backlog, inventory, and threats | documents 01–07 | documented | maintain consistency as domain is implemented |
| Distinct operating models | document 01 | documented | validate with more than one NGO |
| Governance and stakeholders | documents 02–03 | documented | stage 02 |
| Social services | documents 02–03 | documented | delimit sensitive scope |
| Project and impact | 01, 03, 06 | documented | indicator method |
| Funding and instruments | 02–04 | documented | fundraising validation |
| Finance and traceability | document 04 | documented | accountant/finance |
| Budget, commitments, and payments | document 04 and FR-07–09 | documented | validate formulas |
| Authorization and segregation | documents 03–05, FR-10 | documented | threshold policy |
| Reconciliation and accountability | document 04, FR-11/12 | documented | report samples |
| Audit and evidence | documents 03–05 | documented | retention and review |
| Explicit accounting boundary | document 04, ADR-D03 | provisional product boundary | review with accountant after stable release |
| LGPD, privacy, and security | document 05 | documented | legal/DPO review |
| Operational offline | document 05, FR-14 | documented | scenarios and window policy |
| MVP and deferred capabilities | document 06 | documented | product approval |
| Decision owner and evidence | this document | documented | close blocked ADRs |
| Terminology with NGO finance professional | no record/evidence in repository | pending | interview/recorded review |
| Obligations with qualified accountant/advisor | no record/evidence in repository | pending | documented review |

## Required validation

Before marking the stage accepted, record date, participants, anonymized
organization or authorization to identify it, questions, answers, decisions,
disagreements, and pending items. Minimum questions are:

1. How does the NGO distinguish approved, received, allocated, committed, and paid?
2. Which instruments require budget items, documents, returns, or amendments?
3. How are shared costs and reallocations authorized?
4. What evidence links statement, payment, beneficiary, project, and accountability?
5. Which reports does the accountant consider management versus statutory?
6. Which records may be captured offline and which require connectivity?

Without this evidence, internal documentation must not be declared acceptance
of stage 01.
