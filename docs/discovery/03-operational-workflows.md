# Operational workflows

The workflows below describe business situations, not endpoints or tables.
Each must receive states, rules, and domain tests in later stages.

## Control map by workflow

| Workflow | Decisions and rules | Exceptions | Evidence and result | Authorization, audit, and offline | Relationships |
|---|---|---|---|---|---|
| Governance and access | validity, scope, and delegation | revocation, authority conflict | approved act and history | competent administrator; trail; no offline scope expansion | person/service, funding |
| Person, family, and service | eligibility, purpose, and exit | duplication, missing guardian, version conflict | participation, attendance, and authorized document | scoped team; sensitive access audited; later offline capture | project/impact |
| Project and impact | plan, indicator, and closure criterion | delay, changed target, insufficient evidence | versioned activity, observation, and evidence | coordinator reviews; authorship and time; offline draft | service, funding, accountability |
| Fundraising and funding | eligibility, approval, instrument, and installments | rejection, partial award, delay, amendment, return | proposal, decision, agreement, and proof | segregated leadership/finance; connected final submission | project, budget, operation |
| Reconciliation and accountability | matching, period, and snapshot | duplication, no match, rejection | closed session and reproducible package | distinct reconciler/reviewer; connected closing | funding, evidence, audit |

## Governance and access

**Objective:** keep organization, units, responsibilities, and authority
valid. **Actors:** leadership/board, administrator, unit manager, and auditor.
**Preconditions:** NGO installation and authority policy recorded. **Trigger:**
creation, change, or revocation of responsibility.

1. The administrator records the change and its validity.
2. The competent authority approves when policy requires it.
3. The system applies unit/project scope and records actor, date, reason, and
   evidence.
4. Future operations re-evaluate authorization on the server.

The requester may not approve their own change when segregation is required. A
revocation does not erase history. Offline, the change remains pending and
cannot locally expand authority without validation.

## Person, family, and service

**Objective:** record the minimum necessary participation safely. **Actors:**
social/education team, participant, guardian, and coordinator. **Preconditions:**
program, criteria, and location are current.

1. The team identifies or locates an existing identity.
2. It records family/guardianship relationship only when necessary.
3. It assesses eligibility and records enrollment/participation or referral.
4. It records activity, attendance, service/benefit, and authorized evidence.
5. It updates exit, transfer, or closure with reason and date.

Duplicates must be flagged, not silently resolved. Sensitive notes require a
specific purpose and permission. Photos and documents remain outside the
relational database, with an authorized reference. Attendance may be captured
offline and synchronized later; conflicting changes require review.

## Project and impact

**Objective:** execute a project and demonstrate results without confusing
delivery with impact. **Actors:** proposer, coordinator, delivery team, funder,
and evaluator.

1. The proposal defines problem, audience, objective, term, and expected results.
2. The approved project receives a plan, activities, milestones, owners, and risks.
3. The team records execution and evidence with author and date.
4. Indicator observations record method, period, source, and quality.
5. The coordinator reviews deviations and the funder receives the applicable report.
6. Closure checks deliverables, outstanding work, balance, and lessons learned.

Counting workshops is an activity/output indicator; it does not by itself prove
outcome or impact. Each indicator's method must be defined and reviewed.

## Fundraising and funding

**Objective:** track an opportunity through receipt and obligations. **Actors:**
fundraising, leadership, funder, legal/accounting, and finance.

1. The team records opportunity, eligibility, deadline, and required documents.
2. It assesses fit and decides whether to prepare a proposal.
3. It submits a proposal with versioned budget and plan.
4. It records full, partial, or rejected approval.
5. It formalizes instrument, conditions, installments, restrictions, and deliverables.
6. It records each expected and received disbursement, including delay,
   difference, return, or cancellation.
7. It links execution, allocations, expenses, and accountability to the instrument.

Approval is not receipt; a promise is not cash. A change to term, amount,
budget item, or obligation must create an amendment/version and a new review.

## Synthetic financial scenarios

### Restricted donation received

A company promises R$ 30,000 for reading workshops. The opportunity or
relationship is recorded, the restriction is validated, the donation is
approved, and the instrument defines the period. Only after confirmed credit
to the account is the entry recognized as received. The resource is allocated
to the project and permitted budget items; the receipt and proof are evidence.
An expense outside the restriction must be blocked or routed to an authorized
exception.

### Delayed and partial public installment

An agreement provides three installments of R$ 40,000. The second arrives with
R$ 25,000 after the deadline. The system preserves expected amount, received
amount, balance, delay, and communication, without turning the deficit into an
expense or erasing the schedule. The project may be reforecast, but the change
requires a decision and history.

### Shared cost

A R$ 1,000 invoice serves two projects. The NGO approves a documented 50%/50%
method, creates two allocations totaling the transaction, and preserves the
original evidence. The same payment cannot consume cash twice; the allocation
rule is reproducible.

### Service without connectivity

An educator records attendance and activity on site without internet. The
device shows that registration and permission may be stale, stores the local
operation, and schedules synchronization. The server revalidates user, unit,
version, and conflict. The screen does not present a pending record as final
confirmation.

### Supplier payment and reversal

A R$ 2,400 payment is requested with invoice, project, and budget item. A
different approver validates eligibility and the executor records the account
outflow. If the bank returns the payment, the original transaction remains
posted, the return references it, and the budget is recalculated according to
policy; historical value is not silently changed.

### Reconciliation and rejected accountability

The reconciler imports a synthetic statement, matches lines to movements, and
closes the period with an explained difference. Accountability uses that
closing, allocations, and documents. If the funder rejects an item, the reason
remains on the original submission; the team corrects it in a new version,
reprocesses totals, and preserves the relationship between versions.

## Cross-cutting rules

- Every sensitive transition has actor, date, reason, previous state, and applicable evidence.
- Logical deletion/archiving does not destroy necessary history.
- Synchronization failures are visible and repeatable without duplicating effects.
- Rejected accountability keeps package and reason; correction creates a new
  version and resubmission, not silent replacement.
