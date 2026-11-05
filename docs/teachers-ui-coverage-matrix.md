# Traceability and UI Coverage Matrix - Teachers Feature

This document provides the endpoint-by-endpoint traceability matrix and architectural coverage for the **Teachers** feature in *Horizon – Olhar do Futuro*, adhering to offline-first and single-tenant NGO architecture, centralized Kiota REST consumption (`HF.Client`), MVVM PageModel decoupling (`HF.Shared`), MudBlazor design guidelines, accessibility, keyboard navigation, and cross-platform support across Blazor WebAssembly (`HF.Web`) and .NET MAUI Blazor Hybrid (`HF.Mobile`).

---

## 1. Teachers Feature Traceability Matrix

Controller: `src/HF.API/Controllers/v1/TeachersController.cs`

| # | HTTP Method & Route | Authorization Policy | Request Payload / Query | Response / Status Codes | Shared UI Page & Action | Scoped PageModel | Centralized Service | Kiota Operation | Coverage Status |
|---|---|---|---|---|---|---|---|---|---|
| 1 | `GET /api/v1/teachers` | `[Authorize(TeachersRead)]` | Query: `schoolUnitId` (string?), `status` (TeacherStatus?), `specialty` (string?) | `200 OK` (`IReadOnlyList<TeacherResponse>`), `401 Unauthorized`, `403 Forbidden` | `TeacherList.razor`<br>Filter inputs and table load | `TeacherListPageModel.LoadTeachersAsync()` | `ITeacherService.ListAsync(filter, ct)` | `ApiClient.Api.V1.Teachers.GetAsync(...)` | **Covered & Verified** |
| 2 | `GET /api/v1/teachers/{id}` | `[Authorize(TeachersRead)]` | Route: `id` (string) | `200 OK` (`TeacherResponse`), `401 Unauthorized`, `403 Forbidden`, `404 NotFound` | `TeacherDetails.razor`<br>`TeacherEdit.razor` | `TeacherDetailsPageModel.LoadTeacherAsync(id)`<br>`TeacherEditPageModel.LoadTeacherAsync(id)` | `ITeacherService.GetByIdAsync(id, ct)` | `ApiClient.Api.V1.Teachers[id].GetAsync(...)` | **Covered & Verified** |
| 3 | `POST /api/v1/teachers` | `[Authorize(TeachersCreate)]` | Body: `CreateTeacherRequest` (`UserId`, `EmployeeNumber`, `Specialty`, `AcademicDegree`, `Bio`, `SchoolUnitId`, `HireDate`, `Status`) | `201 Created` (`TeacherResponse`), `400 BadRequest`, `401 Unauthorized`, `403 Forbidden`, `422 UnprocessableEntity` | `TeacherCreate.razor`<br>Form submission with client validation | `TeacherCreatePageModel.CreateTeacherAsync()` | `ITeacherService.CreateAsync(request, ct)` | `ApiClient.Api.V1.Teachers.PostAsync(...)` | **Covered & Verified** |
| 4 | `PUT /api/v1/teachers/{id}` | `[Authorize(TeachersUpdate)]` | Route: `id` (string)<br>Body: `UpdateTeacherRequest` (`Id`, `EmployeeNumber`, `Specialty`, `AcademicDegree`, `Bio`, `SchoolUnitId`, `Status`) | `200 OK` (`TeacherResponse`), `400 BadRequest`, `401 Unauthorized`, `403 Forbidden`, `404 NotFound`, `422 UnprocessableEntity` | `TeacherEdit.razor`<br>Form edit and save with validation | `TeacherEditPageModel.SaveTeacherAsync()` | `ITeacherService.UpdateAsync(request, ct)` | `ApiClient.Api.V1.Teachers[id].PutAsync(...)` | **Covered & Verified** |
| 5 | `DELETE /api/v1/teachers/{id}` | `[Authorize(TeachersDelete)]` | Route: `id` (string) | `204 NoContent`, `401 Unauthorized`, `403 Forbidden`, `404 NotFound`, `409 Conflict` | `TeacherDetails.razor`<br>`TeacherList.razor`<br>Destructive confirmation dialog | `TeacherDetailsPageModel.DeleteTeacherAsync()`<br>`TeacherListPageModel.DeleteTeacherAsync(id)` | `ITeacherService.DeleteAsync(id, ct)` | `ApiClient.Api.V1.Teachers[id].DeleteAsync(...)` | **Covered & Verified** |

---

## 2. Shared Razor Pages and MVVM PageModels

Each screen implements dedicated MVVM decoupling, registered as `Scoped` in `ServiceCollectionExtensions.AddPageModels()`:

1. **`TeacherList.razor` & `TeacherListPageModel`** (`/teachers`):
   - Protected with `@attribute [Authorize]`
   - Client-side and server-side filtering by `Status` (Active, OnLeave, Inactive), `SchoolUnitId`, and `Specialty`.
   - Real-time client-side search across employee number, specialty, academic degree, and school unit.
   - Responsive `MudTable` with sorting, paging, loading indicators, empty data state, and row actions.
   - Explicit alert banners for Error, Success, Forbidden (`Teachers.Read`), and Unauthorized (401).

2. **`TeacherCreate.razor` & `TeacherCreatePageModel`** (`/teachers/create`):
   - Protected with `@attribute [Authorize]`
   - Inputs for `UserId` (required), `EmployeeNumber` (required, max 50), `Specialty` (required, max 100), `AcademicDegree` (optional), `SchoolUnitId` (optional), `HireDate` (`MudDatePicker`), `Status` (`MudSelect`), and `Bio` (multiline).
   - Validates constraints prior to HTTP dispatch and maps server validation errors (`ProblemDetails` 422 / 400).

3. **`TeacherDetails.razor` & `TeacherDetailsPageModel`** (`/teachers/{Id}`):
   - Protected with `@attribute [Authorize]`
   - Displays full teacher profile, status badges, educational degrees, operational units, and audit metadata (`CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`).
   - Handles `404 NotFound` with a dedicated, user-friendly card and recovery navigation.
   - Provides edit navigation and confirmation dialog for deletion.

4. **`TeacherEdit.razor` & `TeacherEditPageModel`** (`/teachers/{Id}/edit`):
   - Protected with `@attribute [Authorize]`
   - Pre-fills current attributes for editing, validates constraints, and sends `PUT /api/v1/teachers/{id}`.
   - Handles `404 NotFound` and concurrency/conflict (`409 Conflict`) states.

---

## 3. Automated Test Coverage

The feature is validated by 41 dedicated automated tests in `tests/HF.UITests`:

- `SharedTeachersPageModelsTests`: 19 tests validating all state transitions, validations, filters, CRUD operations, and error mappings.
- `SharedTeachersRouteAndComponentDiscoveryTests`: 3 tests validating `@page` routing, explicit `[Authorize]` attribute decorations, and DI registration.
- `SharedTeachersKiotaIntegrationTests`: 19 tests validating HTTP serialization, query parameter formatting, problem details mapping (200, 201, 204, 400, 401, 403, 404, 409, 422), and Bearer authorization injection.

