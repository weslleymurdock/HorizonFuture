# 07 — Project budgets, restrictions, and resource allocation

## Objective
Enable each NGO to plan, allocate, monitor, and control financial resources by project, funding source, period, and approved rubric.

## Dependencies
Stages 04–06.

## Work
- Model project budgets and versioned budget revisions, with approval status, validity, and immutable history of approved versions.
- Model budget lines/rubrics with quantity/unit where relevant, planned value, period, justification, and funder mapping.
- Model funding allocations from one or more sources to projects and rubrics; prevent the same money from being allocated beyond its available/approved amount.
- Model restricted/unrestricted funds, eligible/ineligible expense rules, spending windows, caps, matching requirements, and remanejamento/transfer approval rules.
- Model commitments/reservations and planned obligations separately from paid expenses to avoid misleading available-balance calculations.
- Define shared-cost allocation across projects/sources with transparent allocation methods and traceable calculation basis.
- Define budget amendments, reforecasting, approval workflow, alerts for thresholds/overspend/underspend, and remaining-balance semantics.
- Specify planned-versus-committed-versus-realized calculations and how reversals/refunds affect them.

## Acceptance criteria
- Budget can be traced to approved funding source(s), project, rubric, and period.
- Multiple funders per project and partial source allocation are supported.
- Budget revisions preserve prior approved state and approvals.
- Automated domain tests cover over-allocation, restricted expenses, expired funds, shared costs, and budget revision scenarios.

## Risks and open decisions
Funders may impose different rules for carry-forward, reallocation, matching, and eligible costs. Rules must be configurable and versioned per agreement.
