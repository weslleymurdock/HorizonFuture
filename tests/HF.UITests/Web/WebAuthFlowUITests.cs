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
/// Web UI automation tests for Blazor WebAssembly (HF.Web) covering Authentication flows using Selenium WebDriver.
/// Executes observable browser interactions with real inputs, gradual human typing, and visual verification.
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
            : 200;

        _typeDelayMs = int.TryParse(Environment.GetEnvironmentVariable("HF_UI_TEST_TYPE_DELAY_MS"), out var typeDelay)
            ? typeDelay
            : 20;

        _highlightEnabled = !bool.TryParse(Environment.GetEnvironmentVariable("HF_UI_TEST_ENABLE_HIGHLIGHT"), out var highlight) || highlight;
    }

    private IWebDriver CreateWebDriver()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")))
        {
            Environment.SetEnvironmentVariable("DISPLAY", ":0");
        }

        var options = new ChromeOptions();
        options.AddArguments("--no-sandbox", "--disable-dev-shm-usage", "--disable-gpu", "--window-size=1280,850");

        if (string.Equals(Environment.GetEnvironmentVariable("HF_UI_TEST_HEADLESS"), "true", StringComparison.OrdinalIgnoreCase))
        {
            options.AddArgument("--headless=new");
        }

        if (File.Exists("/opt/google/chrome/google-chrome"))
        {
            options.BinaryLocation = "/opt/google/chrome/google-chrome";
        }
        else if (File.Exists("/usr/bin/google-chrome"))
        {
            options.BinaryLocation = "/usr/bin/google-chrome";
        }

        options.SetLoggingPreference(LogType.Performance, LogLevel.All);

        try
        {
            return new ChromeDriver(options);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to initialize Google Chrome / ChromeDriver. Details: {ex.Message}", ex);
        }
    }

    [Fact]
    public void LoginFlow_ValidatesInputs_AndSubmits_ObservableVisualBrowser()
    {
        _output.WriteLine($"[TEST SETUP] Web URL: {_webBaseUrl} | API URL: {_apiBaseUrl}");
        _output.WriteLine($"[TEST SETUP] Action Delay: {_actionDelayMs}ms | Type Delay: {_typeDelayMs}ms | Highlight: {_highlightEnabled}");

        using var driver = CreateWebDriver();

        try
        {
            // 1. Navigate to login page
            var loginUrl = $"{_webBaseUrl}/auth/login";
            _output.WriteLine($"[UI ACTION] Navigating to {loginUrl}...");
            driver.Navigate().GoToUrl(loginUrl);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            var emailInput = wait.Until(d =>
            {
                var el = d.FindElement(By.CssSelector("input[type='email']"));
                return (el.Displayed && el.Enabled) ? el : null;
            }) ?? throw new NoSuchElementException("Email field not found or not enabled.");

            var passwordInput = driver.FindElement(By.CssSelector("input[type='password']"));

            Assert.NotNull(emailInput);
            Assert.NotNull(passwordInput);

            // 2. Fill email field
            _output.WriteLine("[UI ACTION] Focusing and populating Email field...");
            HumanType(driver, emailInput, _adminEmail);

            // 3. Fill password field (without logging password)
            _output.WriteLine("[UI ACTION] Focusing and populating Password field...");
            HumanType(driver, passwordInput, _adminPassword);

            // 4. Locate submit button and click
            _output.WriteLine("[UI ACTION] Locating active submit button in DOM...");
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
            }) ?? throw new NoSuchElementException("Login button not found or not enabled.");

            _output.WriteLine("[UI ACTION] Highlighting and clicking submit button...");
            HumanClick(driver, submitButton);

            // 5. Explicit wait for visible result in UI
            _output.WriteLine("[UI WAIT] Waiting for authentication result in interface...");
            var resultFound = wait.Until(d =>
            {
                var successAlerts = d.FindElements(By.CssSelector(".mud-alert-filled-success, .mud-alert"));
                foreach (var alert in successAlerts)
                {
                    if (alert.Displayed && alert.Text.Contains("sucesso", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                var errorAlerts = d.FindElements(By.CssSelector(".mud-alert-filled-error, .mud-alert-message"));
                foreach (var alert in errorAlerts)
                {
                    if (alert.Displayed && !string.IsNullOrWhiteSpace(alert.Text))
                    {
                        throw new InvalidOperationException($"Authentication error displayed in UI: {alert.Text}");
                    }
                }

                return false;
            });

            Assert.True(resultFound, "Success message should be displayed in interface.");
            _output.WriteLine("[UI SUCCESS] Login success notification observed in UI!");

            // 6. Network audit captured by browser
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
            try
            {
                ClearAllHighlights(driver);
            }
            catch { }
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
            catch { }
        }

        if (observedRequestUrl != null)
        {
            _output.WriteLine($"[NETWORK AUDIT] Request detected: {observedRequestMethod} {observedRequestUrl}");
            _output.WriteLine($"[NETWORK AUDIT] HTTP status received: {observedResponseStatusCode}");

            Assert.Equal("POST", observedRequestMethod);
            Assert.Contains("/api/v1/auth/login", observedRequestUrl);
            Assert.StartsWith(_apiBaseUrl, observedRequestUrl, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(200, observedResponseStatusCode);
        }
        else
        {
            _output.WriteLine("[NETWORK AUDIT] Request not captured in performance logs or processed directly.");
        }
    }

    private void HumanType(IWebDriver driver, IWebElement element, string text)
    {
        HighlightElement(driver, element);
        if (_actionDelayMs > 0) Thread.Sleep(_actionDelayMs);

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

        element.SendKeys(Keys.Tab);

        if (_actionDelayMs > 0) Thread.Sleep(_actionDelayMs);
        RemoveHighlight(driver, element);
    }

    private void HumanClick(IWebDriver driver, IWebElement element)
    {
        HighlightElement(driver, element);
        if (_actionDelayMs > 0) Thread.Sleep(_actionDelayMs);

        try
        {
            element.Click();
        }
        catch
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
        }

        if (_actionDelayMs > 0) Thread.Sleep(_actionDelayMs);
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
        catch { }
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
        catch { }
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
        catch { }
    }
}
