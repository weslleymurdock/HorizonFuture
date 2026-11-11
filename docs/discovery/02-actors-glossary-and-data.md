# Actors, glossary, and data inventory

## Identity and roles

A person or institution should be able to play more than one role over time: an
educator may be an employee and volunteer; a company may be a partner and
funder; a participant may also be another person's family guardian. Discovery
recommends reusable identity, role associations with validity, scope, and
history, evidence-based deduplication, and auditable merges. This is guidance
for stage 02, not a final model.

The authenticated user, the person/institution identity, and the operational
association are not necessarily the same record. Authorization must consider
user, role, unit, program/project, record ownership, and data sensitivity. A
login account must not be required for every beneficiary, donor, or guardian.

## Canonical glossary

| Term | Meaning in this discovery |
|---|---|
| Organization | NGO/OSC served by an independent installation |
| Unit/location | Internal physical or operational scope; it is not a tenant |
| Stakeholder | Person or institution that participates, receives, provides, or supervises |
| Role | Stakeholder's contextual and time-bound relationship with the organization |
| Beneficiary/participant | Person who receives or participates in a service/program |
| Program/service | Ongoing offering or service line of the mission |
| Activity | Executed action with date, owner, participants, and evidence |
| Project | Time-bound structure combining objectives, plan, resources, and results |
| Indicator | Measurement rule; activity, output, outcome, and impact are not synonyms |
| Funding source | Origin/economic classification of a received or expected resource |
| Provider/funder | Person or institution making the resource available |
| Opportunity/call | Fundraising possibility with criteria and deadline |
| Application/proposal | Request submitted for an opportunity |
| Award/grant | Approval decision, total or partial, still subject to an instrument |
| Instrument | Agreement/contract establishing purpose, term, and obligations |
| Installment/disbursement | Amount expected or actually disbursed on a date |
| Received resource | Confirmed deposit into an account/cash; it is not an approval |
| Financial account | Where money is held, such as a bank account or cash |
| Category/chart of accounts | What a movement represents for management |
| Budget item | Approved limit applicable to project, source, and period |
| Allocation | Authorized distribution of a resource to a project/budget item |
| Commitment | Approved obligation or reserve, not necessarily paid yet |
| Transaction/movement | Realized financial fact or obligation event according to the workflow |
| Evidence | Document or record supporting a claim or movement |
| Reconciliation | Controlled correspondence between statement and internal records |
| Accountability/reporting | Submitted package demonstrating use and results |
| Reversal/adjustment | Explicit correction preserving the original fact and history |

## Initial data inventory

| Group | Examples | Initial classification | Required controls |
|---|---|---|---|
| Organization | legal registration, contacts, units, policies | institutional | administrative access and history |
| People/families | identification, relationships, guardians, participation | personal; potentially sensitive | minimization, scope, and restricted access |
| Children/adolescents | age, attendance, authorizations | highly sensitive personal | guardian, purpose, audit, and retention |
| Social service | eligibility, referral, delivery, notes | personal/sensitive depending on content | necessity, segregation, and scope review |
| Project/impact | objectives, activities, indicators, evidence | operational/confidential | authorship, version, source, and correction |
| Fundraising | funder, opportunity, proposal, evaluation | relationship/confidential | role- and time-based access |
| Finance | accounts, sources, budget, commitments, movements | financial/confidential | maker-checker, immutability, and trail |
| Accountability | periods, budget items, documents, submissions | financial/documentary | snapshot, integrity, and retention |
| Documents/media | contract, receipt, photo, statement | varies; external media | metadata, hash, authorization, and expiry |
| Security/audit | login, access, change, approval | restricted | integrity, retention, and controlled querying |
| Device/synchronization | registration, cursor, pending operation | technical/confidential | revocation, idempotency, and no exposure |

Classifications are a starting point. Legal basis, retention, consent
necessity, sharing, and data-subject response require legal and operational
review; they are not presumed by this inventory.

## Existing models and discovery interpretation

The current repository has `Student`, `Teacher`, `Volunteer`, `Collaborator`,
`Director`, and `Manager` as separate entities, each necessarily associated
with `UserId`; it also has courses, subjects, enrollments, attendance, and
sessions, plus `Asset` and `Liability`. These models evidence the current
educational scope, not proof that future NGO identity must remain duplicated.

Guidance for later stages:

- preserve the educational usefulness of courses, enrollment, and attendance;
- investigate how student, guardian, educator, collaborator, and volunteer can
  relate without duplication;
- treat assets and liabilities as financial/property concepts to reconcile,
  not as complete transactions;
- do not refactor these types at this stage;
- consider the mandatory login dependency a hypothesis to revisit for
  participants and external stakeholders.
