# 02 — Organization, governance, people, and stakeholders

## Objective
Model the NGO's organizational structure and the people and institutions that participate in its operation. An installation serves one NGO; school units and departments are internal scopes, not tenants.

## Dependencies
Stage 01.

## Work
- Define the NGO profile, legal/registration identifiers, addresses, contacts, governance bodies, units, departments, and operating locations.
- Model users, roles, capabilities, invitations, memberships, approval authority, and scoped access to units/programs/projects.
- Model reusable stakeholder identities and roles: beneficiaries/participants, family members/guardians, employees, educators, volunteers, donors, suppliers, funders, partners, and public bodies. A person or institution may hold multiple roles without duplicate identity records.
- Define contact/address/document records with restricted access and field-level sensitivity where justified.
- Define volunteer and staff engagement records only to the extent needed by the product; payroll and full HR are not assumed.
- Define lifecycle, merge/deduplication, archival, consent/legal basis, and audit semantics.
- Define stable offline-generated identifiers and ownership/authorization relationships.

## Acceptance criteria
- ER/domain diagrams and glossary cover NGO, units, users, roles, people, institutions, and stakeholder-role associations.
- Permission matrix covers create/read/update/archive and sensitive-data access.
- Identity deduplication and merge rules are documented.
- Domain tests cover invariants once implementation begins.

## Risks and open decisions
Do not turn every stakeholder role into a separate duplicated person table. Avoid collecting sensitive personal data without a defined operational purpose.
