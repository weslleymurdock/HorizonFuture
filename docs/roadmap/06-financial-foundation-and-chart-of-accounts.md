# 06 — Financial foundation, accounts, and classification

## Objective
Establish a coherent financial domain for NGO operations, not a generic sales/ERP module. Every financial value must have traceable origin, classification, period, and authorization.

## Dependencies
Stages 01–05.

## Work
- Define financial periods, currency policy (BRL for initial rollout unless discovery requires otherwise), monetary precision/rounding, and immutable posted-value semantics.
- Model cash, bank, savings, investment, and petty-cash accounts; account ownership, opening/closing, status, and reconciliation capability.
- Model a configurable chart of accounts / financial categories separating income, expense, transfer, asset, and liability classifications as appropriate to management reporting.
- Model project/funder-specific budget rubrics and sub-rubrics, including mappings to the NGO's common categories and funder-specific nomenclature.
- Distinguish financial account (where money is held), classification/category (what a movement represents), funding source (where money came from), project (what it supports), and budget rubric (which approved limit applies).
- Define payment methods, counterparties, document/evidence references, and financial transaction lifecycle (draft, submitted, approved, posted, reversed/voided).
- Decide whether a formal double-entry general ledger is in scope now or whether the first release is an auditable operational cash/budget subledger. Document consequences and migration path; do not claim statutory accounting compliance.
- Define accounting/export integration boundary for the NGO's external accountant.

## Acceptance criteria
- Financial glossary and conceptual model distinguish accounts, categories, sources, projects, rubrics, and transactions.
- Money values use decimal-safe precision and validated invariants; no floating-point money.
- Posting, reversal, and audit semantics are documented.
- Scope explicitly states that the platform supports management and accountability, not replacement of an accountant/statutory books unless separately approved.

## Risks and open decisions
Double-entry accounting, tax/accounting reports, multi-currency, and bank API integrations materially expand scope and require specialist review.
