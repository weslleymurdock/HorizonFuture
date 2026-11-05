# HF.UITests - UI and Integration Test Suite (Web, Mobile, and Shared)

This project contains the comprehensive user interface (UI) and workflow integration test suite for the **Horizon – Olhar do Futuro** platform, covering the **Authentication (Auth)**, **Courses**, and **Subjects** features.

It exercises both client host targets:

- **Web Platform**: Blazor WebAssembly (`HF.Web`) via Selenium WebDriver.
- **Mobile Platform**: .NET MAUI Blazor Hybrid (`HF.Mobile`) via Appium WebDriver and UiAutomator2.
- **Shared Layer (`HF.Shared`)**: Unit, PageModel, and Kiota REST integration tests running against deterministic fake services and mock HTTP adapters.

---

## Project Structure

```text
tests/HF.UITests/
├── Mobile/                                   # Mobile UI automation for HF.Mobile (.NET MAUI Blazor Hybrid)
│   ├── MobileAuthFlowUITests.cs              # Authentication flows and viewport layout
│   ├── MobileCoursesFlowUITests.cs           # Courses list, create, and details views in BlazorWebView
│   ├── MobileSubjectsFlowUITests.cs          # Subjects list, create, and details views in BlazorWebView
│   └── MobileTestCollection.cs               # Sequential collection definition disabling parallel device sessions
├── Shared/                                   # Observable behaviors, PageModels, and Kiota integration
│   ├── Fakes/
│   │   ├── FakeAuthService.cs                # Deterministic fake for IAuthService
│   │   ├── FakeCourseService.cs              # Deterministic fake for ICourseService
│   │   └── FakeSubjectService.cs             # Deterministic fake for ISubjectService
│   ├── SharedAuthKiotaIntegrationTests.cs    # Auth Kiota REST client integration tests
│   ├── SharedAuthPageModelsTests.cs          # Unit tests for the 8 Auth PageModels
│   ├── SharedAuthRouteAndComponentDiscoveryTests.cs # Route and DI resolution for Auth pages
│   ├── SharedCoursesKiotaIntegrationTests.cs # Courses Kiota REST client integration tests (all 5 endpoints)
│   ├── SharedCoursesPageModelsTests.cs       # Unit tests for the 4 Courses PageModels
│   ├── SharedCoursesRouteAndComponentDiscoveryTests.cs # Route and DI resolution for Courses pages
│   ├── SharedSubjectsKiotaIntegrationTests.cs# Subjects Kiota REST client integration tests (all 5 endpoints)
│   ├── SharedSubjectsPageModelsTests.cs      # Unit tests for the 4 Subjects PageModels
│   ├── SharedSubjectsRouteAndComponentDiscoveryTests.cs# Route and DI resolution for Subjects pages
│   └── WebClientBaseUrlConfigurationTests.cs # Verification of REST client base URL resolution
├── Web/                                      # Web UI automation for HF.Web (Selenium WebDriver)
│   ├── WebAuthFlowUITests.cs                 # Real browser authentication and network audit
│   ├── WebCoursesFlowUITests.cs              # Real browser CRUD, validation, details, and deletion for Courses
│   └── WebSubjectsFlowUITests.cs             # Real browser CRUD, validation, details, and deletion for Subjects
├── xunit.runner.json                         # Test runner configuration (parallelization controls)
└── README.md
```

---

## 1. `HF.Shared` Tests (PageModels, Kiota Integration, and Routing)

The `HF.Shared` test suite validates:

- **Razor Component Routing**: Verifies all `@page` route definitions (`/courses`, `/courses/create`, `/courses/{Id}`, `/courses/{Id}/edit`, `/subjects`, `/subjects/create`, `/subjects/{Id}`, `/subjects/{Id}/edit`, `/auth/*`).
- **Dependency Injection Scope**: Verifies that `AddPageModels()` registers every PageModel and service interface (`ICourseService`, `ISubjectService`, `IAuthService`) as `Scoped`.
- **PageModel State Machine**: Tests validation rules, loading states (`IsBusy`), error notifications (`ErrorMessage`), unauthorized/forbidden states (`IsUnauthorized`, `IsForbidden`), and success indicators.
- **Kiota REST Integration**: Validates serialization, query string formatting, error problem details mapping (HTTP 400, 401, 403, 404, 409, 422), and token injection using mock HTTP handlers.

### Executing Shared Tests

```bash
dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~Shared"
```

To run feature-specific shared tests:

```bash
# Courses shared tests
dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~SharedCourses"

# Subjects shared tests
dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~SharedSubjects"
```

---

## 2. `HF.Web` Tests (Selenium WebDriver)

The `HF.Web` tests execute real end-to-end browser operations in Google Chrome against Blazor WebAssembly and the running backend API.

### Tested Workflows

- **Unauthorized States**: Accessing list routes without an active session displays an unauthenticated warning banner.
- **Form Validation**: Submitting invalid or incomplete forms displays inline error alerts.
- **Creation Flow**: Completing forms with synthetic test data generates records, sends HTTP requests via Kiota, and renders success alerts.
- **Details Inspection**: Verifies metadata display (name, code, workload hours, status, course association).
- **Edit and Save**: Updates entity attributes, handles concurrency, and persists changes.
- **Not Found States**: Accessing nonexistent GUID routes displays the dedicated "Not Found" card.
- **Deletion Flow**: Triggers confirmation dialogs, deletes records via the API, and redirects to list views.

### Prerequisites

1. **Google Chrome**: Installed on the host (`/usr/bin/google-chrome` or `/opt/google/chrome/google-chrome`).
2. **Backend API (`HF.API`)**: Running and healthy via Docker Compose or `dotnet run`:

   ```bash
   docker compose up -d db hf
   ```

   Accessible at `http://localhost:8080`.
3. **Web Client (`HF.Web`)**: Running dev server serving WebAssembly assets:

   ```bash
   dotnet run --project src/HF.Web --no-launch-profile --urls "http://localhost:5076"
   ```

### Configuration & Environment Variables

| Variable | Description | Default |
| :--- | :--- | :--- |
| `HF_WEB_BASE_URL` | Base URL of the Blazor WebAssembly application | `http://localhost:5076` |
| `HF_API_BASE_URL` | Base URL of the backend REST API | `http://localhost:8080` |
| `HF_UI_TEST_HEADLESS` | Whether to run Chrome in headless mode (`true`/`false`) | `true` (or unset for GUI) |
| `HF_TEST_ADMIN_EMAIL` | Synthetic admin test email seeded in PostgreSQL | `weslley@horizonfuture.local` |
| `HF_TEST_ADMIN_PASSWORD` | Synthetic admin test password | `p@55W0RD4dm1n` |
| `DISPLAY` | X11/Xwayland display identifier | `:0` |

### Executing Web Tests

```bash
# Run all Web tests in headless mode
HF_UI_TEST_HEADLESS=true dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~Web"

# Run Courses Web tests
HF_UI_TEST_HEADLESS=true dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~WebCoursesFlowUITests"

# Run Subjects Web tests
HF_UI_TEST_HEADLESS=true dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~WebSubjectsFlowUITests"
```

---

## 3. `HF.Mobile` Tests (Appium WebDriver)

The `HF.Mobile` tests validate the MAUI Blazor Hybrid application hosted inside Android's native `BlazorWebView`.

### Workflows

- **Viewport Layout**: Ensures MudBlazor layouts adapt to mobile dimensions inside the native container.
- **Touch-Friendly Forms**: Verifies that input components, buttons, and touch targets are present and responsive.
- **Metadata Cards**: Verifies that card containers, frame elements, and details headers render properly on mobile screens.

### Test Prerequisites

1. **Appium 2.x / 3.x**:

   ```bash
   npm install -g appium
   appium driver install uiautomator2
   ```

2. **Android SDK and Device/Emulator**:
   - Android SDK configured (`ANDROID_HOME`).
   - Connected physical Android device or running emulator verified via `adb devices`.
3. **Installed Mobile App**:
   - Package `com.companyname.hf.mobile` with activity `crc640ad8fb5823e64b54.MainActivity`.
   - Install command:

     ```bash
     dotnet build src/HF.Mobile/HF.Mobile.csproj -f net10.0-android -t:Install
     ```

4. **Appium Server Running**:

   ```bash
   appium --port 4723
   ```

### Test Configuration & Environment Variables

| Variable | Description | Default |
| :--- | :--- | :--- |
| `APPIUM_SERVER_URL` | URL of the Appium automation server | `http://127.0.0.1:4723/` |

### Executing Mobile Tests

Mobile tests belong to the `[Collection("MobileUI")]` with sequential execution configured in `xunit.runner.json` to prevent concurrent session conflicts on a single connected Android device.

```bash
# Run all Mobile tests
dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~Mobile"

# Run Courses Mobile tests
dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~MobileCoursesFlowUITests"

# Run Subjects Mobile tests
dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~MobileSubjectsFlowUITests"
```

---

## 4. Verification Summary

| Test Category | Test Class | Total Tests | Status |
| :--- | :--- | :---: | :---: |
| **Shared Auth** | `SharedAuthPageModelsTests`, `SharedAuthRouteAndComponentDiscoveryTests`, `SharedAuthKiotaIntegrationTests` | 55 | Passed |
| **Shared Courses** | `SharedCoursesPageModelsTests`, `SharedCoursesRouteAndComponentDiscoveryTests`, `SharedCoursesKiotaIntegrationTests` | 54 | Passed |
| **Shared Subjects** | `SharedSubjectsPageModelsTests`, `SharedSubjectsRouteAndComponentDiscoveryTests`, `SharedSubjectsKiotaIntegrationTests` | 57 | Passed |
| **Web Selenium** | `WebAuthFlowUITests`, `WebCoursesFlowUITests`, `WebSubjectsFlowUITests`, `WebClientBaseUrlConfigurationTests` | 15 | Passed |
| **Mobile Appium** | `MobileAuthFlowUITests`, `MobileCoursesFlowUITests`, `MobileSubjectsFlowUITests` | 10 | Passed |
| **Domain & Unit** | `HF.UnitTests` (Entities, Value Objects, Handlers, Seed) | 52 | Passed |
| **Total Verified** | **All Test Projects** | **243** | **0 Failed, 0 Skipped** |
