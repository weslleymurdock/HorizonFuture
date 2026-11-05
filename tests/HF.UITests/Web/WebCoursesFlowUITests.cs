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
/// Web UI automation tests for Blazor WebAssembly (HF.Web) covering the Courses feature using Selenium WebDriver.
/// Executes observable browser interactions with real flows (CRUD, filters, and error states).
/// </summary>
public class WebCoursesFlowUITests
{
    private readonly ITestOutputHelper _output;
    private readonly string _webBaseUrl;
    private readonly string _apiBaseUrl;
    private readonly string _adminEmail;
    private readonly string _adminPassword;

    public WebCoursesFlowUITests(ITestOutputHelper output)
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
    public void CoursesList_WithoutAuth_DisplaysUnauthorizedState()
    {
        using var driver = CreateWebDriver();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

        driver.Navigate().GoToUrl($"{_webBaseUrl}/courses");

        var alert = wait.Until(d =>
        {
            var alerts = d.FindElements(By.CssSelector(".mud-alert, .mud-alert-message"));
            return alerts.FirstOrDefault(a => a.Displayed && a.Text.Contains("não está autenticado", StringComparison.OrdinalIgnoreCase));
        });

        Assert.NotNull(alert);
        Assert.Contains("não está autenticado", alert.Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CoursesList_LoadsCourses_AndNavigatesToCreate()
    {
        using var driver = CreateWebDriver();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

        AuthenticateSession(driver, wait);
        ClientNavigate(driver, "/courses");

        var newCourseBtn = wait.Until(d =>
        {
            var btns = d.FindElements(By.CssSelector("button, a"));
            return btns.FirstOrDefault(b => b.Displayed && b.Text.Contains("Novo Curso", StringComparison.OrdinalIgnoreCase));
        });

        Assert.NotNull(newCourseBtn);
        newCourseBtn.Click();

        var createHeader = wait.Until(d =>
        {
            var texts = d.FindElements(By.CssSelector(".mud-typography-h5, h5"));
            return texts.FirstOrDefault(t => t.Displayed && t.Text.Contains("Cadastrar Novo Curso", StringComparison.OrdinalIgnoreCase));
        });

        Assert.NotNull(createHeader);
    }

    [Fact]
    public void CourseCreate_ValidatesAndSubmitsForm()
    {
        using var driver = CreateWebDriver();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

        AuthenticateSession(driver, wait);
        ClientNavigate(driver, "/courses/create");

        // Wait for form inputs using specific aria-labels
        var nameInput = wait.Until(d =>
        {
            var el = d.FindElement(By.CssSelector("input[aria-label='Nome do curso']"));
            return (el.Displayed && el.Enabled) ? el : null;
        });
        Assert.NotNull(nameInput);

        var codeInput = driver.FindElement(By.CssSelector("input[aria-label='Código do curso']"));
        Assert.NotNull(codeInput);

        // Submit empty form to trigger validation
        var submitBtn = wait.Until(d =>
        {
            return d.FindElements(By.CssSelector("button[type='submit'], button"))
                .FirstOrDefault(b => b.Displayed && b.Text.Contains("Cadastrar Curso", StringComparison.OrdinalIgnoreCase));
        });
        Assert.NotNull(submitBtn);
        submitBtn.Click();

        // Verify validation error
        var errorAlert = wait.Until(d =>
        {
            var alerts = d.FindElements(By.CssSelector(".mud-alert-filled-error, .mud-alert"));
            return alerts.FirstOrDefault(a => a.Displayed && a.Text.Contains("nome do curso é obrigatório", StringComparison.OrdinalIgnoreCase));
        });
        Assert.NotNull(errorAlert);

        // Fill form with valid synthetic course data
        var syntheticCode = $"TEST-{Guid.NewGuid():N}"[..10].ToUpperInvariant();
        TypeText(driver, nameInput, $"Curso Sintetico {syntheticCode}");
        TypeText(driver, codeInput, syntheticCode);

        // Submit form
        submitBtn = driver.FindElements(By.CssSelector("button[type='submit'], button"))
            .First(b => b.Displayed && b.Text.Contains("Cadastrar Curso", StringComparison.OrdinalIgnoreCase));
        ClickElement(driver, submitBtn);

        // Verify success notification
        var successAlert = wait.Until(d =>
        {
            var alerts = d.FindElements(By.CssSelector(".mud-alert-filled-success, .mud-alert"));
            return alerts.FirstOrDefault(a => a.Displayed && a.Text.Contains("cadastrado com sucesso", StringComparison.OrdinalIgnoreCase));
        });

        Assert.NotNull(successAlert);
        Assert.Contains(syntheticCode, successAlert.Text);
    }

    [Fact]
    public void CourseDetails_NotFound_DisplaysNotFoundCard()
    {
        using var driver = CreateWebDriver();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

        AuthenticateSession(driver, wait);
        ClientNavigate(driver, "/courses/non-existent-guid-9999");

        var notFoundTitle = wait.Until(d =>
        {
            var elements = d.FindElements(By.CssSelector(".mud-typography-h5, h5"));
            return elements.FirstOrDefault(e => e.Displayed && e.Text.Contains("Curso Não Encontrado", StringComparison.OrdinalIgnoreCase));
        });

        Assert.NotNull(notFoundTitle);
    }

    [Fact]
    public void CourseCompleteFlow_Create_Details_Edit_Delete()
    {
        using var driver = CreateWebDriver();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

        AuthenticateSession(driver, wait);

        // 1. Navigate to Create
        ClientNavigate(driver, "/courses/create");
        var nameInput = wait.Until(d => d.FindElement(By.CssSelector("input[aria-label='Nome do curso']")));
        var codeInput = driver.FindElement(By.CssSelector("input[aria-label='Código do curso']"));

        var uniqueCode = $"FLOW-{Guid.NewGuid():N}"[..10].ToUpperInvariant();
        TypeText(driver, nameInput, $"Curso Fluxo {uniqueCode}");
        TypeText(driver, codeInput, uniqueCode);

        var submitBtn = driver.FindElements(By.CssSelector("button[type='submit'], button"))
            .First(b => b.Displayed && b.Text.Contains("Cadastrar Curso", StringComparison.OrdinalIgnoreCase));
        ClickElement(driver, submitBtn);

        // Verify success and click "Ver Detalhes do Curso"
        var detailsLinkBtn = wait.Until(d =>
        {
            return d.FindElements(By.CssSelector("button"))
                .FirstOrDefault(b => b.Displayed && b.Text.Contains("Ver Detalhes do Curso", StringComparison.OrdinalIgnoreCase));
        });
        Assert.NotNull(detailsLinkBtn);
        detailsLinkBtn.Click();

        // 2. Details Page
        var detailsTitle = wait.Until(d =>
        {
            var elements = d.FindElements(By.CssSelector(".mud-typography-h4, h4"));
            return elements.FirstOrDefault(e => e.Displayed && e.Text.Contains($"Curso Fluxo {uniqueCode}", StringComparison.OrdinalIgnoreCase));
        });
        Assert.NotNull(detailsTitle);

        // Click Edit
        var editBtn = wait.Until(d =>
        {
            return d.FindElements(By.CssSelector("button"))
                .FirstOrDefault(b => b.Displayed && b.Text.Contains("Editar", StringComparison.OrdinalIgnoreCase));
        });
        Assert.NotNull(editBtn);
        editBtn.Click();

        // 3. Edit Page: update workload
        var workloadInput = wait.Until(d => d.FindElement(By.CssSelector("input[aria-label='Carga horária em horas']")));
        TypeText(driver, workloadInput, "75");

        var saveBtn = driver.FindElements(By.CssSelector("button[type='submit'], button"))
            .First(b => b.Displayed && b.Text.Contains("Salvar Alterações", StringComparison.OrdinalIgnoreCase));
        ClickElement(driver, saveBtn);

        var editSuccessAlert = wait.Until(d =>
        {
            var alerts = d.FindElements(By.CssSelector(".mud-alert-filled-success, .mud-alert"));
            return alerts.FirstOrDefault(a => a.Displayed && a.Text.Contains("atualizado com sucesso", StringComparison.OrdinalIgnoreCase));
        });
        Assert.NotNull(editSuccessAlert);

        // Click "Ver Detalhes Atualizados"
        var updatedDetailsBtn = wait.Until(d =>
        {
            return d.FindElements(By.CssSelector("button"))
                .FirstOrDefault(b => b.Displayed && b.Text.Contains("Ver Detalhes Atualizados", StringComparison.OrdinalIgnoreCase));
        });
        Assert.NotNull(updatedDetailsBtn);
        updatedDetailsBtn.Click();

        // 4. Details Page: Delete Course
        var deleteBtn = wait.Until(d =>
        {
            return d.FindElements(By.CssSelector("button"))
                .FirstOrDefault(b => b.Displayed && b.Text.Contains("Excluir", StringComparison.OrdinalIgnoreCase));
        });
        Assert.NotNull(deleteBtn);
        deleteBtn.Click();

        // Confirm Dialog
        var confirmBtn = wait.Until(d =>
        {
            return d.FindElements(By.CssSelector(".mud-dialog button"))
                .FirstOrDefault(b => b.Displayed && b.Text.Contains("Sim, Excluir", StringComparison.OrdinalIgnoreCase));
        });
        Assert.NotNull(confirmBtn);
        confirmBtn.Click();

        // Verify redirection back to /courses
        wait.Until(d => d.Url.Contains("/courses"));
    }
}
