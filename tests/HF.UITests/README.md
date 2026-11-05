# HF.UITests - Testes de UI (Web, Mobile e Shared)

Este projeto contém a suíte de testes de interface de usuário (UI) e integração de fluxo para a feature de **Autenticação (Auth)** do Horizon – Olhar do Futuro, abrangendo tanto a plataforma Web (Blazor WebAssembly via Selenium) quanto a plataforma Mobile (.NET MAUI Blazor Hybrid via Appium), além de testes de integração observáveis dos comportamentos compartilhados em `HF.Shared`.

---

## Estrutura do Projeto

```
tests/HF.UITests/
├── Shared/                                   # Comportamentos observáveis e resolução DI compartilhada
│   ├── Fakes/
│   │   └── FakeAuthService.cs                # Mock determinístico do IAuthService para testes de PageModel
│   ├── SharedAuthPageModelsTests.cs          # Testes de unidade e fluxo dos 8 PageModels da UI Auth
│   └── SharedAuthRouteAndComponentDiscoveryTests.cs # Validação de rotas @page e resolução Scoped via AddPageModels()
├── Web/                                      # Testes de UI para HF.Web (Selenium WebDriver)
│   └── WebAuthFlowUITests.cs                 # Fluxos de login, cadastro, recuperação de senha, perfil, roles e permissões
├── Mobile/                                   # Testes de UI para HF.Mobile (Appium WebDriver)
│   └── MobileAuthFlowUITests.cs              # Automação no BlazorWebView mobile (Android/iOS)
└── README.md
```

---

## 1. Testes de `HF.Shared` (Integração e Contratos Observáveis)

Os testes de `HF.Shared` validam:
- A correta exportação de todas as 8 páginas Razor com suas rotas correspondentes (`/auth/login`, `/auth/register`, `/auth/forgot-password`, `/auth/reset-password`, `/auth/confirm-email`, `/auth/manage`, `/auth/manage-roles`, `/auth/manage-permissions`).
- A resolução de cada PageModel como serviço `Scoped` através de `services.AddPageModels()`, garantindo compatibilidade direta com `HF.Web` e `HF.Mobile`.
- As regras de validação de formulários, estados de carregamento (`IsBusy`), tratamento de erros e respostas de sucesso.

### Como Executar
```bash
dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~Shared"
```

---

## 2. Testes de `HF.Web` (Selenium WebDriver)

Os testes de `HF.Web` cobrem a interação com componentes MudBlazor em navegador web:
- Validação e submissão do formulário de Login (com e sem 2FA).
- Validação de senhas divergentes e registro de usuário.
- Fluxo de recuperação e redefinição de senha com código.
- Confirmação de email e reenvio.
- Telas administrativas de gerenciamento de Roles, Permissões e Perfil/2FA.

### Pré-requisitos
1. **Google Chrome / Chromium**: Instalado no ambiente (`google-chrome` ou `chromium-browser`).
2. **ChromeDriver**: Versão compatível com a versão instalada do Chrome no PATH do sistema.
3. **Servidor Web e API**:
   - `HF.API` em execução (ex: `http://localhost:5268`).
   - `HF.Web` em execução (ex: `http://localhost:5000`).

### Variáveis de Ambiente
- `HF_WEB_BASE_URL`: URL base do Blazor Web (padrão: `http://localhost:5000`).

### Como Executar
```bash
dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~Web"
```
*Nota: Se o ChromeDriver não for detectado no ambiente, os testes com Selenium utilizarão `Assert.Skip` informando os requisitos pendentes sem falha falsa de compilação.*

---

## 3. Testes de `HF.Mobile` (Appium WebDriver)

Os testes de `HF.Mobile` validam o comportamento do app em viewport móvel e dentro do `BlazorWebView` híbrido no Android.

### Pré-requisitos
1. **Node.js e Appium 2.x**:
   ```bash
   npm install -g appium
   appium driver install uiautomator2
   ```
2. **Android SDK e Emulador**:
   - Android SDK instalado com ferramentas de build e plataforma API 24+.
   - Emulador Android em execução (`emulator -avd <nome_do_avd>`) ou dispositivo físico conectado via ADB (`adb devices`).
3. **App HF.Mobile**:
   - Compilado e instalado no emulador: `com.companyname.hf.mobile`.
4. **Servidor Appium ativo**:
   ```bash
   appium --port 4723
   ```

### Variáveis de Ambiente
- `APPIUM_SERVER_URL`: URL do servidor Appium (padrão: `http://127.0.0.1:4723/`).

### Como Executar
```bash
dotnet test tests/HF.UITests/HF.UITests.csproj --filter "FullyQualifiedName~Mobile"
```
*Nota: Se o servidor Appium ou emulador não estiverem ativos, os testes utilizarão `Assert.Skip` indicando a ação necessária para inicialização do ambiente móvel.*

