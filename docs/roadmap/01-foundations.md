# 01 — Product discovery and domain requirements

## Objective
Define the product scope for an open-source management platform for Brazilian NGOs/OSCs, expanding beyond stakeholder CRUD into social-program delivery, funding lifecycle, and end-to-end financial resource management. This is discovery, not implementation evidence.

## Current status
Internal discovery artifacts are documented under
[docs/discovery](../discovery/README.md). Stage acceptance remains **pending**:
the repository contains no recorded review of financial terminology by a real
NGO finance stakeholder or of reporting obligations by an accountant/advisor.
Documentation completion is therefore not external validation or stage
acceptance.

## Dependencies
None.

## Work
- Map NGO profiles, governance, units, staff, volunteers, beneficiaries/families, donors, suppliers, funders, partners, and their relationships.
- Map the lifecycle from strategic planning and project design through funding applications, awards/agreements, execution, financial control, evidence, accountability, and impact reporting.
- Interview finance, project coordination, educators/social workers, executive management, board members, and external accountants/funders where possible.
- Catalogue financial scenarios: unrestricted and restricted donations, grants, public/private agreements, multiple funders per project, transfers, reimbursements, in-kind donations, recurring commitments, cash/bank accounts, petty cash, shared costs, refunds, and returned funds.
- Define approval limits, segregation of duties, evidence requirements, funding restrictions, reporting calendars, and the difference between operational management and statutory accounting.
- Define offline use cases and identify which financial actions may be created offline, which require approval/server validation, and how stale balances are presented.
- Establish data classification, LGPD purposes/legal bases, minimization, retention, access, audit, export, and deletion requirements.
- Benchmark publicly documented competitor capabilities without copying proprietary implementation or assuming undocumented behavior.

## Acceptance criteria
- Approved domain glossary, actor/persona matrix, workflow maps, prioritized capability backlog, data inventory, and privacy/threat assessment.
- Explicit MVP boundary and deferred capabilities are agreed.
- Financial terminology and reporting obligations are validated with at least one real NGO finance stakeholder and an accountant/advisor.
- Every unresolved decision has an owner and evidence needed.

## Risks and open decisions
The actual NGO operating model, funding instruments, accounting practices, legal obligations, approval policies, and offline duration must be discovered. Avoid designing around a single NGO's exceptional workflow.
