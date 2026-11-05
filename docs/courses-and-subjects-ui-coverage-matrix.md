# Traceability and UI Coverage Matrix - Courses and Subjects Features

This document provides the endpoint-by-endpoint traceability matrix and architectural coverage for the **Courses** and **Subjects** features in *Horizon – Olhar do Futuro*, adhering to offline-first and single-tenant NGO architecture, centralized Kiota REST consumption (`HF.Client`), MVVM PageModel decoupling (`HF.Shared`), MudBlazor design guidelines, and cross-platform support across Blazor WebAssembly (`HF.Web`) and .NET MAUI Blazor Hybrid (`HF.Mobile`).

---

## 1. Courses Feature Traceability Matrix

Controller: `src/HF.API/Controllers/v1/CoursesController.cs`

| # | HTTP Method & Route | Authorization Policy | Request Payload / Query | Response / Status Codes | Shared UI Page & Action | Scoped PageModel | Centralized Service | Kiota Operation | Coverage Status |
|---|---|---|---|---|---|---|---|---|---|
| 1 | `GET /api/v1/courses` | `[Authorize(CoursesRead)]` | Query: `schoolUnitId` (string?), `status` (CourseStatus?) | `200 OK` (`IReadOnlyList<CourseResponse>`), `401 Unauthorized`, `403 Forbidden` | `CourseList.razor`<br>Filter inputs and table load | `CourseListPageModel.LoadCoursesAsync()` | `ICourseService.ListAsync(filter, ct)` | `ApiClient.Api.V1.Courses.GetAsync(...)` | **Covered & Verified** |
| 2 | `GET /api/v1/courses/{id}` | `[Authorize(CoursesRead)]` | Route: `id` (string) | `200 OK` (`CourseResponse`), `401 Unauthorized`, `403 Forbidden`, `404 NotFound` | `CourseDetails.razor`<br>`CourseEdit.razor` | `CourseDetailsPageModel.LoadCourseAsync(id)`<br>`CourseEditPageModel.LoadCourseAsync(id)` | `ICourseService.GetByIdAsync(id, ct)` | `ApiClient.Api.V1.Courses[id].GetAsync(...)` | **Covered & Verified** |
| 3 | `POST /api/v1/courses` | `[Authorize(CoursesCreate)]` | Body: `CreateCourseRequest` (`Name`, `Code`, `WorkloadHours`, `Description`, `SchoolUnitId`, `TargetAudience`, `Status`) | `201 Created` (`CourseResponse`), `400 BadRequest`, `401 Unauthorized`, `403 Forbidden`, `422 UnprocessableEntity` | `CourseCreate.razor`<br>Form submit button | `CourseCreatePageModel.CreateCourseAsync()` | `ICourseService.CreateAsync(request, ct)` | `ApiClient.Api.V1.Courses.PostAsync(...)` | **Covered & Verified** |
| 4 | `PUT /api/v1/courses/{id}` | `[Authorize(CoursesUpdate)]` | Route: `id` (string)<br>Body: `UpdateCourseRequest` (`Name`, `Code`, `WorkloadHours`, `Description`, `SchoolUnitId`, `TargetAudience`, `Status`) | `200 OK` (`CourseResponse`), `400 BadRequest`, `401 Unauthorized`, `403 Forbidden`, `404 NotFound`, `422 UnprocessableEntity` | `CourseEdit.razor`<br>Form save button | `CourseEditPageModel.SaveCourseAsync()` | `ICourseService.UpdateAsync(request, ct)` | `ApiClient.Api.V1.Courses[id].PutAsync(...)` | **Covered & Verified** |
| 5 | `DELETE /api/v1/courses/{id}` | `[Authorize(CoursesDelete)]` | Route: `id` (string) | `204 NoContent`, `401 Unauthorized`, `403 Forbidden`, `404 NotFound`, `409 Conflict` | `CourseDetails.razor`<br>`CourseList.razor`<br>Destructive confirmation dialog | `CourseDetailsPageModel.DeleteCourseAsync()`<br>`CourseListPageModel.DeleteCourseAsync(id)` | `ICourseService.DeleteAsync(id, ct)` | `ApiClient.Api.V1.Courses[id].DeleteAsync(...)` | **Covered & Verified** |

---

## 2. Subjects Feature Traceability Matrix

Controller: `src/HF.API/Controllers/v1/SubjectsController.cs`

| # | HTTP Method & Route | Authorization Policy | Request Payload / Query | Response / Status Codes | Shared UI Page & Action | Scoped PageModel | Centralized Service | Kiota Operation | Coverage Status |
|---|---|---|---|---|---|---|---|---|---|
| 1 | `GET /api/v1/subjects` | `[Authorize(SubjectsRead)]` | Query: `courseId` (string?), `schoolUnitId` (string?), `status` (SubjectStatus?) | `200 OK` (`IReadOnlyList<SubjectResponse>`), `401 Unauthorized`, `403 Forbidden` | `SubjectList.razor`<br>Search field, status & course filter, table load | `SubjectListPageModel.LoadSubjectsAsync()` | `ISubjectService.ListAsync(filter, ct)` | `ApiClient.Api.V1.Subjects.GetAsync(...)` | **Covered & Verified** |
| 2 | `GET /api/v1/subjects/{id}` | `[Authorize(SubjectsRead)]` | Route: `id` (string) | `200 OK` (`SubjectResponse`), `401 Unauthorized`, `403 Forbidden`, `404 NotFound` | `SubjectDetails.razor`<br>`SubjectEdit.razor` | `SubjectDetailsPageModel.LoadSubjectAsync(id)`<br>`SubjectEditPageModel.LoadSubjectAsync(id)` | `ISubjectService.GetByIdAsync(id, ct)` | `ApiClient.Api.V1.Subjects[id].GetAsync(...)` | **Covered & Verified** |
| 3 | `POST /api/v1/subjects` | `[Authorize(SubjectsCreate)]` | Body: `CreateSubjectRequest` (`Name`, `Code`, `WorkloadHours`, `Description`, `CourseId`, `SchoolUnitId`, `Status`) | `201 Created` (`SubjectResponse`), `400 BadRequest`, `401 Unauthorized`, `403 Forbidden`, `422 UnprocessableEntity` | `SubjectCreate.razor`<br>Form submission with validation | `SubjectCreatePageModel.CreateSubjectAsync()` | `ISubjectService.CreateAsync(request, ct)` | `ApiClient.Api.V1.Subjects.PostAsync(...)` | **Covered & Verified** |
| 4 | `PUT /api/v1/subjects/{id}` | `[Authorize(SubjectsUpdate)]` | Route: `id` (string)<br>Body: `UpdateSubjectRequest` (`Name`, `Code`, `WorkloadHours`, `Description`, `CourseId`, `SchoolUnitId`, `Status`) | `200 OK` (`SubjectResponse`), `400 BadRequest`, `401 Unauthorized`, `403 Forbidden`, `404 NotFound`, `422 UnprocessableEntity` | `SubjectEdit.razor`<br>Form edit and save with validation | `SubjectEditPageModel.SaveSubjectAsync()` | `ISubjectService.UpdateAsync(request, ct)` | `ApiClient.Api.V1.Subjects[id].PutAsync(...)` | **Covered & Verified** |
| 5 | `DELETE /api/v1/subjects/{id}` | `[Authorize(SubjectsDelete)]` | Route: `id` (string) | `204 NoContent`, `401 Unauthorized`, `403 Forbidden`, `404 NotFound`, `409 Conflict` | `SubjectDetails.razor`<br>`SubjectList.razor`<br>Destructive confirmation dialog | `SubjectDetailsPageModel.DeleteSubjectAsync()`<br>`SubjectListPageModel.DeleteSubjectAsync(id)` | `ISubjectService.DeleteAsync(id, ct)` | `ApiClient.Api.V1.Subjects[id].DeleteAsync(...)` | **Covered & Verified** |

---

## 3. Courses Gaps Identified and Resolved Prior to Subjects

1. **Repository Manual Transaction Conflict in EF Core**:
   - *Problem*: `src/HF.Infrastructure/Persistence/Repositories/Repository.cs` explicitly invoked `uow.BeginTransactionAsync()` inside `AddAsync`, `UpdateAsync`, `RemoveAsync`, and `RemoveRangeAsync`. When `NpgsqlRetryingExecutionStrategy` is active, user-initiated transactions throw an `InvalidOperationException`.
   - *Resolution*: Removed redundant manual transaction initiations from generic repository methods; relying instead on EF Core's native atomic `SaveChangesAsync()` execution within the unit of work. Rebuilt Docker container `hf` and verified HTTP 201 Created and HTTP 204 NoContent.

2. **MudForm Form Submission Dispatch in Blazor**:
   - *Problem*: In `CourseCreate.razor` and `CourseEdit.razor`, the `<MudForm @onsubmit="...">` event was never fired because MudBlazor renders a `<div>` element without browser submit event propagation.
   - *Resolution*: Added explicit `OnClick="() => PageModel.CreateCourseAsync()"` and `OnClick="() => PageModel.SaveCourseAsync()"` triggers on submit buttons, accompanied by `Immediate="true"` data bindings to ensure PageModel properties are synchronized immediately on keystroke.

3. **Skipped and Non-functional Selenium Tests**:
   - *Problem*: `WebCoursesFlowUITests.cs` had tests decorated with static skips or incomplete mocks.
   - *Resolution*: Rewrote the entire suite into observable browser tests (`WebCoursesFlowUITests.cs`) testing real flows (authentication, list, create with validation, details view, edit, not found card, and deletion).

4. **Appium Automation on Mobile**:
   - *Problem*: Mobile tests were skipped with outdated activity hashes (`crc64104c8651a5c13636.MainActivity`) and failed when executed simultaneously.
   - *Resolution*: Updated to real package activity `crc640ad8fb5823e64b54.MainActivity`, configured Appium 2.x/3.x with UiAutomator2, deployed updated APK to connected physical Android device, and established sequential execution via `[Collection("MobileUI")]` and `xunit.runner.json`.

---

## 4. UI Architecture and Shared Implementation

- **Location**: All UI components, PageModels, DTOs, and client services reside in `src/HF.Shared/`.
- **Decoupled MVVM**: Each view has an isolated PageModel (`SubjectListPageModel`, `SubjectCreatePageModel`, `SubjectDetailsPageModel`, `SubjectEditPageModel`) registered as `Scoped` via `AddPageModels()` in `ServiceCollectionExtensions.cs`.
- **Kiota REST Consumption**: Neither views nor PageModels make manual HTTP requests or construct URLs. All operations go through `ISubjectService` / `SubjectService` and `ICourseService` / `CourseService`, which encapsulate Kiota's generated `ApiClient`.
- **Error and Loading States**: High-contrast MudAlert banners, MudSkeleton/MudProgressCircular indicators, empty table alerts, 404 cards, and unauthorized prompts are present across all views.
- **Accessibility**: Focus rings (`outline: 3px solid`), ARIA labels (`aria-label`), keyboard navigation (`Keys.Tab`, `Enter`), and contrast standards are adhered to.

