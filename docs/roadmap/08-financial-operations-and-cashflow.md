# 08 — Financial operations, transactions, and cash flow

## Objective
Model the day-to-day movement and obligation lifecycle for all NGO financial resources with traceability and controls.

## Dependencies
Stages 05–07.

## Work
- Model income and expense records, receivables, payables, installments, due dates, payment/receipt events, partial settlement, late status, refunds, chargebacks, and cancellations.
- Model donations (one-time/recurring), donor restrictions, acknowledgements/receipts, and in-kind donations with non-cash valuation and provenance.
- Model transfers between financial accounts, project allocations, and permitted funding buckets without treating transfers as income/expense twice.
- Model transaction splits across projects, sources, rubrics, cost centers/units, and periods; enforce allocation totals and trace each split to the original transaction.
- Model reimbursements, advances, petty cash, supplier/beneficiary payments, fees, interest, and bank charges as confirmed by discovery.
- Define approval workflow, spending limits, maker-checker separation, supporting-document requirements, and exception handling.
- Define immutable posting and correction by reversal/adjustment rather than destructive edits to posted movements.
- Define cash-flow projections based on scheduled receipts/payments and distinguish forecasts from bank/account balances.
- Keep financial attachment binaries in external object storage; store metadata, hash, ownership, and authorization in PostgreSQL.

## Acceptance criteria
- End-to-end scenarios cover donation receipt, grant installment, supplier expense, transfer, split allocation, refund, and reversal.
- Every posted transaction is traceable to counterparty, source, project/rubric allocation, financial account, evidence, and actor where applicable.
- Cash flow distinguishes realized, pending, overdue, and forecast amounts.
- Authorization and approval tests prove segregation of duties and scoped access.

## Risks and open decisions
Payment gateway/bank integrations, payroll, tax withholding, and automated recurring debit are separate integrations and not assumed in the first domain release.
