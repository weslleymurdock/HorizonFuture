# 09 — Reconciliation, accountability, audit, and financial reporting

## Objective
Provide reliable controls and reports that let an NGO explain where each resource came from, how it was used, what remains, and whether obligations were met.

## Dependencies
Stages 05–08.

## Work
- Model bank statement imports, statement lines, matching rules, reconciliation sessions, unmatched items, fees, and reconciliation adjustments.
- Define reconciliation lifecycle and safeguards against duplicate import or matching a transaction more than once.
- Model accountability/reporting periods and deliverables per agreement/funder, required documents, rubrics, deadlines, review, submission, and acceptance/rejection.
- Generate planned-versus-committed-versus-realized reports by project, funding source, account, category/rubric, period, and counterparty.
- Generate cash/bank statements, cash flow, source-of-funds traceability, budget execution, outstanding commitments, and supporting-document packages.
- Model audit events and approval history with actor, timestamp, reason, before/after references, and correlation to source records.
- Define exports (CSV/XLSX/PDF) and reproducibility/versioning of submitted reports; report totals must be traceable to source transactions.
- Define alerts for budget thresholds, overdue receivables/payables, missing evidence, upcoming reporting deadlines, and unreconciled statements.
- Define data retention and access for financial documents and audit trails.

## Acceptance criteria
- A synthetic grant can be reconciled and its accountability package produced from source records.
- Report totals reconcile with transaction/allocation records and are reproducible for a closed period.
- Bank import and reconciliation are idempotent and auditable.
- A submitted accountability report preserves the exact snapshot/version and supporting evidence used.

## Risks and open decisions
Bank file formats, funder-specific report layouts, electronic signatures, and accounting exports need prioritization. Never imply that management reports are audited statutory financial statements.
