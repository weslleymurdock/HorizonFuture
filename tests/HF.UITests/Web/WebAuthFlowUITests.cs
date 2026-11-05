using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;
using Xunit.Abstractions;

namespace HF.UITests.Web;

/// <summary>
/// Testes de automação de UI para Blazor WebAssembly (HF.Web) utilizando Selenium WebDriver.
/// Executado em modo visual (não headless) com ritmo gradual e destaque visual dos elementos.
/// </summary>
public class WebAuthFlowUITests
{
    private readonly ITestOutputHelper _output;
    private readonly string _webBaseUrl;
    private readonly string _apiBaseUrl;
    private readonly string _adminEmail;
    private readonly string _adminPassword;
    private readonly int _actionDelayMs;
    private readonly int _typeDelayMs;
    private readonly bool _highlightEnabled;

    public WebAuthFlowUITests(ITestOutputHelper output)
    {
        _output = output;
        _webBaseUrl = (Environment.GetEnvironmentVariable("HF_WEB_BASE_URL") ?? "http://localhost:5076").TrimEnd('/');
        _apiBaseUrl = (Environment.GetEnvironmentVariable("HF_API_BASE_URL") ?? "http://localhost:8080").TrimEnd('/');
        _adminEmail = Environment.GetEnvironmentVariable("HF_TEST_ADMIN_EMAIL")
            ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL")
            ?? "weslley@horizonfuture.local";
        _adminPassword = Environment.GetEnvironmentVariable("HF_TEST_ADMIN_PASSWORD")
            ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
            ?? "p@55W0RD4dm1n";

        _actionDelayMs = int.TryParse(Environment.GetEnvironmentVariable("HF_UI_TEST_ACTION_DELAY_MS"), out var actionDelay)
            ? actionDelay
            : 400;

        _typeDelayMs = int.TryParse(Environment.GetEnvironmentVariable("HF_UI_TEST_TYPE_DELAY_MS"), out var typeDelay)
            ? typeDelay
            : 40;

        _highlightEnabled = !bool.TryParse(Environment.GetEnvironmentVariable("HF_UI_TEST_ENABLE_HIGHLIGHT"), out var highlight) || highlight;
    }

    private IWebDriver CreateWebDriver()
    {
        // Garante DISPLAY para execução gráfica se estiver em ambiente Linux com X11/Xwayland
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")))
        {
            Environment.SetEnvironmentVariable("DISPLAY", ":0");
        }

        var options = new ChromeOptions();

        // MODO VISUAL: NÃO adicionar --headless
        options.AddArguments("--no-sandbox", "--disable-dev-shm-usage", "--disable-gpu", "--window-size=1280,850");

        if (File.Exists("/opt/google/chrome/google-chrome"))
        {
            options.BinaryLocation = "/opt/google/chrome/google-chrome";
        }
        else if (File.Exists("/usr/bin/google-chrome"))
        {
            options.BinaryLocation = "/usr/bin/google-chrome";
        }

        // Habilita captura de eventos de rede para auditoria do tráfego sem expor credenciais
        options.SetLoggingPreference(LogType.Performance, LogLevel.All);

        try
        {
            return new ChromeDriver(options);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Falha ao inicializar o Google Chrome / ChromeDriver em modo visual. " +
                $"Certifique-se de que o Google Chrome está instalado e que um display gráfico (como DISPLAY=:0) está disponível. " +
                $"Detalhes: {ex.Message}", ex);
        }
    }

    [Fact]
    public void LoginFlow_ValidatesInputs_AndSubmits_ObservableVisualBrowser()
    {
        _output.WriteLine($"[TEST SETUP] Web URL: {_webBaseUrl} | API URL: {_apiBaseUrl}");
        _output.WriteLine($"[TEST SETUP] Action Delay: {_actionDelayMs}ms | Type Delay: {_typeDelayMs}ms | Highlight: {_highlightEnabled}");

        using var driver = CreateWebDriver();
        var js = (IJavaScriptExecutor)driver;

        try
        {
            // 1. Navegação para a tela de login
            var loginUrl = $"{_webBaseUrl}/auth/login";
            _output.WriteLine($"[UI ACTION] Navegando para {loginUrl}...");
            driver.Navigate().GoToUrl(loginUrl);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            var emailInput = wait.Until(d =>
            {
                var el = d.FindElement(By.CssSelector("input[type='email']"));
                return (el.Displayed && el.Enabled) ? el : null;
            }) ?? throw new NoSuchElementException("Campo de email não encontrado ou não habilitado.");

            var passwordInput = driver.FindElement(By.CssSelector("input[type='password']"));

            Assert.NotNull(emailInput);
            Assert.NotNull(passwordInput);

            // 2. Preenchimento gradual e visual do campo de email
            _output.WriteLine("[UI ACTION] Focando e preenchendo campo de Email...");
            HumanType(driver, emailInput, _adminEmail);

            // 3. Preenchimento gradual e visual do campo de senha (sem logar a senha)
            _output.WriteLine("[UI ACTION] Focando e preenchendo campo de Senha...");
            HumanType(driver, passwordInput, _adminPassword);

            // 4. Localiza o botão ativo no DOM após renderização e clica
            _output.WriteLine("[UI ACTION] Localizando botão Entrar ativo no DOM...");
            var submitButton = wait.Until(d =>
            {
                var buttons = d.FindElements(By.CssSelector("button"));
                foreach (var btn in buttons)
                {
                    if (btn.Displayed && btn.Enabled && btn.Text.Contains("Entrar", StringComparison.OrdinalIgnoreCase))
                    {
                        return btn;
                    }
                }
                return null;
            }) ?? throw new NoSuchElementException("Botão de login não encontrado ou não habilitado.");

            _output.WriteLine("[UI ACTION] Destacando e clicando no botão Entrar...");
            HumanClick(driver, submitButton);

            // 5. Espera explícita pelo resultado visível na UI
            _output.WriteLine("[UI WAIT] Aguardando conclusão da autenticação na interface...");
            var resultFound = wait.Until(d =>
            {
                // Verifica sucesso visível na UI
                var successAlerts = d.FindElements(By.CssSelector(".mud-alert-filled-success, .mud-alert"));
                foreach (var alert in successAlerts)
                {
                    if (alert.Displayed && alert.Text.Contains("sucesso", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                // Verifica erro visível na UI
                var errorAlerts = d.FindElements(By.CssSelector(".mud-alert-filled-error, .mud-alert-message"));
                foreach (var alert in errorAlerts)
                {
                    if (alert.Displayed && !string.IsNullOrWhiteSpace(alert.Text))
                    {
                        throw new InvalidOperationException($"Erro de autenticação exibido na UI: {alert.Text}");
                    }
                }

                return false;
            });

            Assert.True(resultFound, "A mensagem de sucesso da autenticação deve ser exibida na interface.");
            _output.WriteLine("[UI SUCCESS] Notificação de sucesso de login observada na UI!");

            // 6. Auditoria do tráfego HTTP capturado pelo navegador
            VerifyNetworkTraffic(driver);
        }
        catch (Exception ex)
        {
            _output.WriteLine($"[TEST FAILURE] {ex.Message}");
            try
            {
                VerifyNetworkTraffic(driver);
                var browserLogs = driver.Manage().Logs.GetLog(LogType.Browser);
                foreach (var log in browserLogs)
                {
                    _output.WriteLine($"[BROWSER CONSOLE] {log.Level}: {log.Message}");
                }
                var cardText = (string)((IJavaScriptExecutor)driver).ExecuteScript("return document.querySelector('.mud-card')?.innerText || document.body?.innerText || '';");
                _output.WriteLine($"[CARD / BODY TEXT] {cardText}");
            }
            catch { }
            throw;
        }
        finally
        {
            // Limpeza visual de destaques antes de encerrar
            try
            {
                ClearAllHighlights(driver);
            }
            catch
            {
                // Ignora se o browser já estiver fechado
            }
        }
    }

    private void VerifyNetworkTraffic(IWebDriver driver)
    {
        var logs = driver.Manage().Logs.GetLog(LogType.Performance);
        string? observedRequestUrl = null;
        string? observedRequestMethod = null;
        int? observedResponseStatusCode = null;

        foreach (var entry in logs)
        {
            if (string.IsNullOrWhiteSpace(entry.Message)) continue;

            try
            {
                using var doc = JsonDocument.Parse(entry.Message);
                var root = doc.RootElement;
                if (!root.TryGetProperty("message", out var messageProp)) continue;

                if (!messageProp.TryGetProperty("method", out var methodProp)) continue;
                var method = methodProp.GetString();

                if (!messageProp.TryGetProperty("params", out var paramsProp)) continue;

                if (method == "Network.requestWillBeSent")
                {
                    if (paramsProp.TryGetProperty("request", out var requestProp))
                    {
                        var url = requestProp.GetProperty("url").GetString();
                        var httpMethod = requestProp.GetProperty("method").GetString();
                        if (url != null && url.Contains("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase))
                        {
                            observedRequestUrl = url;
                            observedRequestMethod = httpMethod;
                        }
                    }
                }
                else if (method == "Network.responseReceived")
                {
                    if (paramsProp.TryGetProperty("response", out var responseProp))
                    {
                        var url = responseProp.GetProperty("url").GetString();
                        if (url != null && url.Contains("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase))
                        {
                            observedResponseStatusCode = responseProp.GetProperty("status").GetInt32();
                        }
                    }
                }
            }
            catch
            {
                // Logs não-JSON ou irrelevantes
            }
        }

        if (observedRequestUrl != null)
        {
            _output.WriteLine($"[NETWORK AUDIT] Requisição detectada: {observedRequestMethod} {observedRequestUrl}");
            _output.WriteLine($"[NETWORK AUDIT] Status HTTP recebido: {observedResponseStatusCode}");

            Assert.Equal("POST", observedRequestMethod);
            Assert.Contains("/api/v1/auth/login", observedRequestUrl);
            Assert.StartsWith(_apiBaseUrl, observedRequestUrl, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(200, observedResponseStatusCode);
        }
        else
        {
            _output.WriteLine("[NETWORK AUDIT] Requisição não capturada nos logs de performance ou processada diretamente.");
        }
    }

    private void HumanType(IWebDriver driver, IWebElement element, string text)
    {
        HighlightElement(driver, element);
        Thread.Sleep(_actionDelayMs);

        element.Click();
        element.Clear();
        foreach (var ch in text)
        {
            element.SendKeys(ch.ToString());
            if (_typeDelayMs > 0)
            {
                Thread.Sleep(_typeDelayMs);
            }
        }

        // Garante que o evento 'change' seja disparado no input do Blazor/MudBlazor
        element.SendKeys(Keys.Tab);

        Thread.Sleep(_actionDelayMs);
        RemoveHighlight(driver, element);
    }

    private void HumanClick(IWebDriver driver, IWebElement element)
    {
        HighlightElement(driver, element);
        Thread.Sleep(_actionDelayMs);

        try
        {
            element.Click();
        }
        catch
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
        }

        Thread.Sleep(_actionDelayMs);
        RemoveHighlight(driver, element);
    }

    private void HighlightElement(IWebDriver driver, IWebElement element)
    {
        if (!_highlightEnabled) return;

        try
        {
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript(
                "if (arguments[0]) {" +
                "  arguments[0].setAttribute('data-original-outline', arguments[0].style.outline || '');" +
                "  arguments[0].setAttribute('data-original-box-shadow', arguments[0].style.boxShadow || '');" +
                "  arguments[0].style.outline = '3px solid #E91E63';" +
                "  arguments[0].style.boxShadow = '0 0 12px #E91E63';" +
                "  arguments[0].scrollIntoView({ behavior: 'smooth', block: 'center' });" +
                "}",
                element);
        }
        catch
        {
            // Fallback silencioso se o elemento não puder ser estilizado
        }
    }

    private void RemoveHighlight(IWebDriver driver, IWebElement element)
    {
        if (!_highlightEnabled) return;

        try
        {
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript(
                "if (arguments[0]) {" +
                "  arguments[0].style.outline = arguments[0].getAttribute('data-original-outline') || '';" +
                "  arguments[0].style.boxShadow = arguments[0].getAttribute('data-original-box-shadow') || '';" +
                "  arguments[0].removeAttribute('data-original-outline');" +
                "  arguments[0].removeAttribute('data-original-box-shadow');" +
                "}",
                element);
        }
        catch
        {
            // Ignora se o elemento já estiver stale
        }
    }

    private void ClearAllHighlights(IWebDriver driver)
    {
        if (!_highlightEnabled) return;

        try
        {
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript(
                "document.querySelectorAll('[data-original-outline]').forEach(el => {" +
                "  el.style.outline = el.getAttribute('data-original-outline') || '';" +
                "  el.style.boxShadow = el.getAttribute('data-original-box-shadow') || '';" +
                "  el.removeAttribute('data-original-outline');" +
                "  el.removeAttribute('data-original-box-shadow');" +
                "});");
        }
        catch
        {
            // Ignora se driver foi encerrado
        }
    }
}
