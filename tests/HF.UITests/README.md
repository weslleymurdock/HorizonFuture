# HF.UITests - Testes de UI (Web, Mobile e Shared)

This project contains the user interface (UI) and workflow integration test suite for the **Authentication (Auth)** feature of *Horizon – Olhar do Futuro*. It covers both the Web platform (Blazor WebAssembly via Selenium) and the Mobile platform (.NET MAUI Blazor Hybrid via Appium), as well as integration tests for shared behaviors defined in `HF.Shared`.

---

## Project Structure

```text
tests/HF.UITests/
├── Shared/                                   # Observable behaviors and shared DI resolution
│   ├── Fakes/
│   │   └── FakeAuthService.cs                # Deterministic mock of IAuthService for PageModel testing
│   ├── SharedAuthPageModelsTests.cs          # Unit and flow tests for the 8 PageModels in the Auth UI
│   └── SharedAuthRouteAndComponentDiscoveryTests.cs # Validation of @page routes and Scoped resolution via AddPageModels()
├── Web/                                      # UI tests for HF.Web (Selenium WebDriver)
│   └── WebAuthFlowUITests.cs                 # Login, registration, password recovery, profile, roles, and permissions flows
├── Mobile/                                   # UI tests for HF.Mobile (Appium WebDriver)
│   └── MobileAuthFlowUITests.cs              # Automation in BlazorWebView for mobile (Android/iOS)
└── README.md
```

---

## 1. `HF.Shared` Tests (Integration and Observable Contracts)

The `HF.Shared` tests validate:

- The correct export of all 8 Razor Pages, along with their corresponding routes (`/auth/login`, `/auth/register`, `/auth/forgot-password`, `/auth/reset-password`, `/auth/confirm-email`, `/auth/manage`, `/auth/manage-roles`, `/auth/manage-permissions`).
- The resolution of each `PageModel` as a `Scoped` service via `services.AddPageModels()`, ensuring direct compatibility with `HF.Web` and `HF.Mobile`.
- Form validation rules, loading states (`IsBusy`), error handling, and success responses.

### How to Run

```bash
dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~Shared"
```

---

## 2. `HF.Web` Tests (Selenium WebDriver)

The `HF.Web` tests cover interactions with MudBlazor components within a web browser:

- Visible, non-headless login flow with gradual action pacing and high-contrast visual focus highlighting.
- Audit of browser network traffic ensuring requests target the configured API endpoint (`POST /api/v1/auth/login`) with HTTP 200 response.
- Verification of visible success notification without hardcoding or logging sensitive credentials or tokens.
- Integration tests ensuring Kiota base URL resolution never defaults to the Web static host address.

### Prerequisites

1. **Google Chrome / Chromium**: Installed on the host (`/usr/bin/google-chrome` or `/opt/google/chrome/google-chrome`).
2. **Graphical Display**: An active X11 / Xwayland display (`DISPLAY=:0`).
3. **ChromeDriver**: Automatically resolved via Selenium Manager or available in PATH.
4. **Backend API (`HF.API`)**: Running and healthy via Docker Compose or local dotnet CLI:
   - **Docker Compose (Recommended)**:
     ```bash
     docker compose up -d db hf
     ```
     Listens on `http://localhost:8080` (or `http://127.0.0.1:8080`).
   - **Dotnet CLI**:
     ```bash
     dotnet run --project src/HF.API --no-launch-profile --urls "http://localhost:5267"
     ```
5. **Web Client (`HF.Web`)**: Running and serving Blazor WebAssembly static assets:
   ```bash
   dotnet run --project src/HF.Web --no-launch-profile --urls "http://localhost:5076"
   ```

### Configuration & Environment Variables

| Variable | Description | Default |
| :--- | :--- | :--- |
| `HF_WEB_BASE_URL` | Base URL of the Blazor WebAssembly application | `http://localhost:5076` |
| `HF_API_BASE_URL` | Base URL of the backend REST API | `http://localhost:8080` |
| `HF_TEST_ADMIN_EMAIL` | Synthetic test account email (seeded by `DatabaseSeedHostedService`) | `weslley@horizonfuture.local` (or `ADMIN_EMAIL`) |
| `HF_TEST_ADMIN_PASSWORD` | Synthetic test account password | `p@55W0RD4dm1n` (or `ADMIN_PASSWORD`) |
| `HF_UI_TEST_ACTION_DELAY_MS` | Delay between UI actions (navigation, focus, click) for human observation | `400` ms |
| `HF_UI_TEST_TYPE_DELAY_MS` | Delay between characters typed into inputs | `40` ms |
| `HF_UI_TEST_ENABLE_HIGHLIGHT` | Highlights focused elements with a high-contrast pink/magenta border | `true` |

### How to Run

1. **Run the observable visual login test**:
   ```bash
   DISPLAY=:0 HF_WEB_BASE_URL=http://localhost:5076 HF_API_BASE_URL=http://localhost:8080 dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~LoginFlow_ValidatesInputs_AndSubmits_ObservableVisualBrowser" --logger "console;verbosity=normal"
   ```

2. **Run all Web and Base URL configuration tests**:
   ```bash
   dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~WebClientBaseUrlConfigurationTests"
   ```

---

## 3. `HF.Mobile` Tests (Appium WebDriver)

The `HF.Mobile` tests validate the app's behavior within a mobile viewport and inside the hybrid `BlazorWebView` on Android.

### Pre-requisites

1. **Node.js e Appium 2.x**:

   ```bash
   npm install -g appium
   appium driver install uiautomator2
   ```

2. **Android SDK e Emulador**:
   - Android SDK installed with build tools and plataform API 24+.
   - Emulator Android running (`emulator -avd <avd_name>`) or physical device connected through ADB (`adb devices`).
3. **App HF.Mobile**:
   - Built and installed at emulator: `com.remotecodehub.hf.mobile`.
4. **Appium server active**:

   ```bash
   appium --port 4723
   ```

### Environment Variables

- `APPIUM_SERVER_URL`: URL do servidor Appium (padrão: `http://127.0.0.1:4723/`).

### How to Run

```bash
dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~Mobile"
```

*Note: If the Appium server or emulator is not active, the tests will use `Assert.Skip`, indicating the action required to initialize the mobile environment.*
