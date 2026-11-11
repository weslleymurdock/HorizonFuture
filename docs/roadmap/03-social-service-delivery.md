# 03 — Social programs, beneficiaries, and service delivery

## Objective
Represent the social operation delivered by an NGO: who is served, through which programs, services, activities, and attendances, with a traceable history.

## Dependencies
Stages 01–02.

## Work
- Model program/service lines, target audiences, eligibility criteria, locations, and operating periods.
- Model beneficiary and family composition, guardianship/relationships, enrollment/participation, referrals, attendance, appointments, and service/benefit delivery.
- Model activity plans and sessions, facilitators, participant lists, attendance outcomes, notes, and evidence.
- Define configurable forms/custom fields with versioning, validation, access control, and safe offline behavior; do not permit arbitrary untyped blobs to become the domain model.
- Define case/history records only within an approved scope and privacy review; restrict highly sensitive notes.
- Define documents, photos, and evidence metadata while keeping binary media outside PostgreSQL.
- Establish links from delivered services and beneficiaries to programs and funded projects without duplicating records.

## Acceptance criteria
- Lifecycle diagrams cover enrollment through participation, attendance, referral, and exit.
- Authorization matrix protects beneficiary/family records and sensitive notes.
- Required operational reports and offline workflows are specified.
- Domain invariants and representative synthetic scenarios are documented.

## Risks and open decisions
The MVP must not become a clinical/social-case-management system without a deliberate privacy and regulatory scope. Keep sensitive notes optional and tightly controlled.
