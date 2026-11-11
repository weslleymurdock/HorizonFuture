# 02 — Organization, governance, people, and stakeholders

## Objective
Model the NGO's organizational structure and the people and institutions that participate in its operation. An installation serves one NGO; school units and departments are internal scopes, not tenants.

## Dependencies
Stage 01.

## Implemented baseline

The repository now contains the framework-independent Stage 02 domain baseline under
`HF.Domain.Organization`, `HF.Domain.Stakeholders`, and `HF.Domain.Governance`. It includes
organization/unit/department lifecycle, reusable person and institution identities,
organizational memberships with multiple dated roles, explicit authentication-account links,
person relationships, governance mandates, authority assignments, deterministic duplicate
matching, and an approval-tracked identity merge.

The existing educational role records remain operational projections during migration. They
carry an optional `PersonId` reference to the canonical identity while preserving their
education-specific fields and existing `UserId` compatibility links. See
`docs/guidelines/organization-and-stakeholders.md` for the mapping and invariants.

## Stabilization status

The domain baseline was hardened with hierarchy movement and cycle protection, scoped normalized
name uniqueness, archive guards, identity reactivation, membership and relationship transition
guards, Unicode-aware deterministic matching, and merge operator audit fields. A follow-up EF
migration records the new merge audit columns.

Backend acceptance validation is complete. Test execution is an external-environment requirement
for this repository: backend tests must be run outside the sandbox, on a host with the required
.NET SDK and a functioning xUnit adapter.

Required external validation commands:

```powershell
dotnet restore HorizonFuture.slnx
dotnet build HorizonFuture.slnx --no-restore
dotnet test tests\HF.UnitTests\HF.UnitTests.csproj
```

The external execution record includes the host, SDK, exact backend commands, and literal test
totals. UI tests are intentionally excluded because no interface implementation is part of Stage
02.

Backend unit tests are intentionally isolated from `HF.Shared`, `HF.Client`, MAUI, and browser/UI
dependencies. Shared Razor component and route tests live in `HF.UITests`; they are not part of
the Stage 02 backend acceptance command.

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
