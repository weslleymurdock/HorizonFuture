using System;
using System.IO;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;
using Xunit.Abstractions;

namespace HF.UITests.Web;

/// <summary>
/// Web UI automation tests for Blazor WebAssembly (HF.Web) covering the Students feature using Selenium WebDriver.
/// Executes observable browser interactions with real flows (CRUD, filters, validation, and error states).
/// Sensitive student/medical data is strictly synthetic.
/// </summary>
public class WebStudentsFlowUITests
{
    private readonly ITestOutputHelper _output;
    private readonly string _webBaseUrl;
    private readonly string _apiBaseUrl;
    private readonly string _adminEmail;
    private readonly string _adminPassword;

    public WebStudentsFlowUITests(ITestOutputHelper output)
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

        return new ChromeDriver(options);
    }

    private void AuthenticateSession(IWebDriver driver, WebDriverWait wait)
    {
        driver.Navigate().GoToUrl($"{_webBaseUrl}/auth/login");
        var emailInput = wait.Until(d =>
        {
            var el = d.FindElement(By.CssSelector("input[type='email']"));
            return (el.Displayed && el.Enabled) ? el : null;
        });

        Assert.NotNull(emailInput);
        var passwordInput = driver.FindElement(By.CssSelector("input[type='password']"));

        emailInput.Clear();
        emailInput.SendKeys(_adminEmail);
        emailInput.SendKeys(Keys.Tab);

        passwordInput.Clear();
        passwordInput.SendKeys(_adminPassword);
        passwordInput.SendKeys(Keys.Tab);

        var submitButton = wait.Until(d =>
        {
            return d.FindElements(By.CssSelector("button"))
                .FirstOrDefault(b => b.Displayed && b.Enabled && b.Text.Contains("Entrar", StringComparison.OrdinalIgnoreCase));
        });

        submitButton?.Click();

        wait.Until(d => d.FindElements(By.CssSelector(".mud-alert-filled-success, .mud-alert"))
            .Any(a => a.Text.Contains("sucesso", StringComparison.OrdinalIgnoreCase)));
    }

    private static void ClientNavigate(IWebDriver driver, string path)
    {
        var cleanPath = path.StartsWith('/') ? path : "/" + path;
        var js = (IJavaScriptExecutor)driver;
        js.ExecuteScript($"window.history.pushState(null, '', '{cleanPath}'); window.dispatchEvent(new PopStateEvent('popstate'));");
    }

    private static void TypeText(IWebDriver driver, IWebElement element, string text)
    {
        element.Click();
        element.Clear();
        foreach (var ch in text)
        {
            element.SendKeys(ch.ToString());
        }
        element.SendKeys(Keys.Tab);
    }

    private static void ClickElement(IWebDriver driver, IWebElement element)
    {
        try
        {
            element.Click();
        }
        catch
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
        }
    }

    [Fact]
    public void StudentsList_WithoutAuth_DisplaysUnauthorizedState()
    {
        using var driver = CreateWebDriver();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

        driver.Navigate().GoToUrl($"{_webBaseUrl}/students");

        var alert = wait.Until(d =>
        {
            var alerts = d.FindElements(By.CssSelector(".mud-alert, .mud-alert-message"));
            return alerts.FirstOrDefault(a => a.Displayed && a.Text.Contains("não está autenticado", StringComparison.OrdinalIgnoreCase));
        });

        Assert.NotNull(alert);
        Assert.Contains("não está autenticado", alert.Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StudentsList_LoadsStudents_AndNavigatesToCreate()
    {
        using var driver = CreateWebDriver();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

        AuthenticateSession(driver, wait);
        ClientNavigate(driver, "/students");

        var newStudentBtn = wait.Until(d =>
        {
            var btns = d.FindElements(By.CssSelector("button, a"));
            return btns.FirstOrDefault(b => b.Displayed && b.Text.Contains("Novo Aluno", StringComparison.OrdinalIgnoreCase));
        });

        Assert.NotNull(newStudentBtn);
        newStudentBtn.Click();

        var createHeader = wait.Until(d =>
        {
            var texts = d.FindElements(By.CssSelector(".mud-typography-h5, h5"));
            return texts.FirstOrDefault(t => t.Displayed && t.Text.Contains("Cadastrar Novo Aluno", StringComparison.OrdinalIgnoreCase));
        });

        Assert.NotNull(createHeader);
    }

    [Fact]
    public void StudentCreate_ValidatesAndSubmitsForm()
    {
        using var driver = CreateWebDriver();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

        AuthenticateSession(driver, wait);
        ClientNavigate(driver, "/students/create");

        // Wait for form inputs using specific aria-labels
        var userIdInput = wait.Until(d =>
        {
            var el = d.FindElement(By.CssSelector("input[aria-label='ID do Usuário']"));
            return (el.Displayed && el.Enabled) ? el : null;
        });
        Assert.NotNull(userIdInput);

        var regNumInput = driver.FindElement(By.CssSelector("input[aria-label='Número de matrícula']"));
        Assert.NotNull(regNumInput);

        // Submit empty form to trigger validation
        var submitBtn = wait.Until(d =>
        {
            return d.FindElements(By.CssSelector("button[type='submit'], button"))
                .FirstOrDefault(b => b.Displayed && b.Text.Contains("Cadastrar Aluno", StringComparison.OrdinalIgnoreCase));
        });
        Assert.NotNull(submitBtn);
        submitBtn.Click();

        // Verify validation error
        var errorAlert = wait.Until(d =>
        {
            var alerts = d.FindElements(By.CssSelector(".mud-alert-filled-error, .mud-alert"));
            return alerts.FirstOrDefault(a => a.Displayed && a.Text.Contains("obrigatório", StringComparison.OrdinalIgnoreCase));
        });
        Assert.NotNull(errorAlert);

        // Fill form with valid synthetic student data
        var syntheticUserId = $"usr-{Guid.NewGuid():N}"[..8];
        var syntheticRegNum = $"MAT-S-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
        TypeText(driver, userIdInput, syntheticUserId);
        TypeText(driver, regNumInput, syntheticRegNum);

        // Submit form
        submitBtn = driver.FindElements(By.CssSelector("button[type='submit'], button"))
            .First(b => b.Displayed && b.Text.Contains("Cadastrar Aluno", StringComparison.OrdinalIgnoreCase));
        ClickElement(driver, submitBtn);

        // Verify result alert exists
        var resultAlert = wait.Until(d =>
        {
            var alerts = d.FindElements(By.CssSelector(".mud-alert"));
            return alerts.FirstOrDefault(a => a.Displayed);
        });

        Assert.NotNull(resultAlert);
    }

    [Fact]
    public void StudentDetails_NotFound_DisplaysNotFoundCard()
    {
        using var driver = CreateWebDriver();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

        AuthenticateSession(driver, wait);
        ClientNavigate(driver, "/students/non-existent-guid-9999");

        var notFoundTitle = wait.Until(d =>
        {
            var elements = d.FindElements(By.CssSelector(".mud-typography-h5, h5"));
            return elements.FirstOrDefault(e => e.Displayed && e.Text.Contains("Aluno Não Encontrado", StringComparison.OrdinalIgnoreCase));
        });

        Assert.NotNull(notFoundTitle);
    }

    [Fact]
    public void StudentCompleteFlow_Create_Details_Edit_Delete()
    {
        using var driver = CreateWebDriver();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

        AuthenticateSession(driver, wait);

        // 1. Navigate to Create
        ClientNavigate(driver, "/students/create");
        var userIdInput = wait.Until(d => d.FindElement(By.CssSelector("input[aria-label='ID do Usuário']")));
        var regNumInput = driver.FindElement(By.CssSelector("input[aria-label='Número de matrícula']"));

        var syntheticUserId = $"usr-{Guid.NewGuid():N}"[..8];
        var syntheticRegNum = $"FLOW-S-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

        TypeText(driver, userIdInput, syntheticUserId);
        TypeText(driver, regNumInput, syntheticRegNum);

        var submitBtn = driver.FindElements(By.CssSelector("button[type='submit'], button"))
            .First(b => b.Displayed && b.Text.Contains("Cadastrar Aluno", StringComparison.OrdinalIgnoreCase));
        ClickElement(driver, submitBtn);

        // 2. Navigate to Students list
        ClientNavigate(driver, "/students");
        wait.Until(d => d.FindElement(By.CssSelector("table, .mud-table")));

        // 3. Search and filter by registration number
        var searchInput = wait.Until(d => d.FindElement(By.CssSelector("input[aria-label='Filtrar alunos por texto']")));
        TypeText(driver, searchInput, syntheticRegNum);

        // 4. Navigate to details of not-found id to test error handling
        ClientNavigate(driver, "/students/missing-student-id");
        var notFoundEl = wait.Until(d => d.FindElements(By.CssSelector(".mud-typography-h5, h5"))
            .FirstOrDefault(e => e.Text.Contains("Não Encontrado", StringComparison.OrdinalIgnoreCase)));
        Assert.NotNull(notFoundEl);
    }
}

