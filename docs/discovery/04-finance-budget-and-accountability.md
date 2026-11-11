# Finance, budget, and accountability

## Traceability chain

The financial domain must allow navigation in both directions:

`provider → source → opportunity/proposal → award → instrument → installment
→ project → allocation → budget item/budget → commitment → transaction →
account/movement → evidence → reconciliation → accountability → audit`

Not every instrument will have every link, but absence must be explicit and
justified. A transaction may have multiple allocations; a project may have
multiple sources; a source may support multiple projects. Allocation totals
must be verifiable and may not exceed applicable limits without an approved
exception.

## Required distinctions

| Concept | Question answered |
|---|---|
| Provider | who provides or promises the resource? |
| Source | what origin/type and what restrictions exist? |
| Opportunity | through which call can the organization apply? |
| Application | what was submitted? |
| Award | what was approved, fully or partially? |
| Instrument | what legal/operational obligations were formalized? |
| Disbursement | how much should be disbursed and when? |
| Receipt | how much actually entered? |
| Financial account | where is the amount held? |
| Category | what does the movement represent? |
| Budget item | what budget limit governs the expense? |
| Commitment | what obligation was assumed/reserved? |
| Payment | what movement actually left? |
| Accountability | how does the organization demonstrate use and delivery? |

## Conceptual budget and balances

For each project, source, budget item, and period, the organization must distinguish:

- **planned:** spending intent not yet approved;
- **approved:** current limit of the approved version;
- **allocated:** resource portion linked to project/budget item;
- **committed:** approved obligation/reserve;
- **paid/realized:** confirmed financial movement;
- **available:** approved/allocated less commitments according to policy;
- **remaining:** balance after realized amounts and reversal/adjustment effects.

Exact formulas depend on NGO policy and instrument. A payment must not
automatically eliminate commitment, allocation, and balance from every source
without an explicit rule. Reversals, refunds, fees, interest, returns,
matching, carry-forward, and reallocation require versioned treatment.

Budgets must have versions, rationale, approvers, validity, and comparison with
the previous version. The approved version is not overwritten.

## Operations

The product must discover and distinguish accounts receivable/payable,
installments, partial receipts/payments, delays, transfers, advances,
reimbursements, returns, chargebacks, fees, interest, in-kind donations, and
cash forecasts. A transfer between accounts is not revenue or expense twice.
Splits preserve the original transaction and total exactly the applicable
amount, respecting decimal rounding.

Suggested conceptual states for validation: draft, submitted, approved, posted,
reversed/adjusted, canceled. Posted records cannot be destructively edited;
correction references the original fact and requires reason/actor.

## Segregation of duties

Process roles should be distinct where applicable:

`requester → approver → finance operator → payment executor → reconciler → reviewer/auditor`

The requester should not approve their own request, and the executor should not
reconcile their own payment alone when policy requires otherwise. Limits,
delegations, substitutions, and exceptions are configurable and require NGO
validation; this document invents no legal thresholds.

## Reconciliation

1. Import the statement and identify file/period idempotently.
2. Create lines with reference, date, amount, and description.
3. Suggest or record a match to an internal movement.
4. Separate unmatched, duplicate, fee, and adjustment lines.
5. Review, resolve differences, and close the session with an owner.
6. Prevent the same line from being reconciled twice without an explicit correction.

Closing preserves version, evidence, differences, decisions, and period.

## Accountability

A package must preserve instrument, period, budget items, deliverables,
documents, indicators, and the exact snapshot of records used. Minimum flow:
preparation → internal review → submission → acceptance, rejection, or
correction request → new version → closure. Rejection does not erase the
previous submission. Totals are reproducible and point to underlying
transactions, allocations, evidence, and reconciliation.

A management report or accountability package is not automatically an audited
financial statement. The first product version must decide, with an accountant
or advisor, whether it offers an auditable operational subledger and export,
or double-entry accounting. It must not claim to replace statutory accounting
without specialist validation.

## Exceptions to validate

| Case | Expected treatment |
|---|---|
| partial approval | preserve requested, approved, and unapproved amounts without creating a receipt |
| delayed/partial disbursement | keep expected, received, difference, deadline, and communication |
| ineligible expense | block or route to an authorized exception without hiding the expense |
| reallocation | require instrument permission, new approval, and budget-item history |
| return/chargeback | record related movement and impact on allocation/accountability |
| duplicate payment | detect, block, or reverse with reference and reason |
| missing document | mark pending and prevent submission when mandatory |
| rejected accountability | preserve package, reason, correction, and versioned resubmission |
