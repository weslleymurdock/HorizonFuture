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

- Login form validation and submission (with and without 2FA).
- Validation of mismatched passwords and user registration.
- Password recovery and reset flow using a code.
- Email confirmation and resend functionality.
- Administrative screens for managing roles, permissions, and user profiles/2FA.

### Prerequisites

1. **Google Chrome / Chromium**: Must be installed in the environment (`google-chrome` or `chromium-browser`).
2. **ChromeDriver**: A version compatible with the installed Chrome browser, present in the system PATH.
3. **Web Server and API**:

- `HF.API` must be running (e.g., `http://localhost:5268`).
- `HF.Web` must be running (e.g., `http://localhost:5000`).

### Environment Variables

- `HF_WEB_BASE_URL`: The base URL for the Blazor Web application (default: `http://localhost:5000`).

### How to Run

```bash
dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~Web"
```

*Note: If ChromeDriver is not detected in the environment, Selenium tests will use `Assert.Skip` to report the pending requirements, avoiding false build failures.*

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
