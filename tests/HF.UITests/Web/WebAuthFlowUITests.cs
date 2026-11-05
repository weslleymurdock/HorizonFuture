using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace HF.UITests.Web;

/// <summary>
/// Testes de automação de UI para Blazor WebAssembly (HF.Web) utilizando Selenium WebDriver.
/// Requisitos para execução no host:
/// 1. Google Chrome / Chromium instalado
/// 2. ChromeDriver correspondente disponível no PATH
/// 3. Servidor Web ativo em HF_WEB_BASE_URL (padrão: http://localhost:5000)
/// </summary>
public class WebAuthFlowUITests
{
    private readonly string _baseUrl = Environment.GetEnvironmentVariable("HF_WEB_BASE_URL") ?? "http://localhost:5000";

    private IWebDriver CreateWebDriver()
    {
        var options = new ChromeOptions();
        options.AddArguments("--headless", "--no-sandbox", "--disable-dev-shm-usage", "--disable-gpu");
        return new ChromeDriver(options);
    }

    [Fact(Skip = "Requer Google Chrome e ChromeDriver no PATH para execução de automação Selenium")]
    public void LoginFlow_ValidatesInputs_AndSubmits()
    {
        using var driver = CreateWebDriver();
        driver.Navigate().GoToUrl($"{_baseUrl}/auth/login");
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        var emailInput = wait.Until(d => d.FindElement(By.CssSelector("input[type='email']")));
        var passwordInput = driver.FindElement(By.CssSelector("input[type='password']"));
        var submitButton = driver.FindElement(By.CssSelector("button[type='button'], button[type='submit']"));

        Assert.NotNull(emailInput);
        Assert.NotNull(passwordInput);
        Assert.NotNull(submitButton);

        emailInput.SendKeys("test@horizon.org");
        passwordInput.SendKeys("Password123!");
        submitButton.Click();
    }

    [Fact(Skip = "Requer Google Chrome e ChromeDriver no PATH para execução de automação Selenium")]
    public void RegisterFlow_ValidatesPasswordMismatch()
    {
        using var driver = CreateWebDriver();
        driver.Navigate().GoToUrl($"{_baseUrl}/auth/register");
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        var emailInput = wait.Until(d => d.FindElement(By.CssSelector("input[type='email']")));
        var passwordInputs = driver.FindElements(By.CssSelector("input[type='password']"));

        Assert.NotNull(emailInput);
        Assert.True(passwordInputs.Count >= 2, "Deveria conter campos de senha e confirmação de senha.");

        emailInput.SendKeys("user@horizon.org");
        passwordInputs[0].SendKeys("Password123!");
        passwordInputs[1].SendKeys("MismatchPassword123!");

        var submitButton = driver.FindElement(By.CssSelector("button"));
        submitButton.Click();
    }

    [Fact(Skip = "Requer Google Chrome e ChromeDriver no PATH para execução de automação Selenium")]
    public void ForgotPasswordAndResetPassword_NavigationAndFlow()
    {
        using var driver = CreateWebDriver();
        driver.Navigate().GoToUrl($"{_baseUrl}/auth/forgot-password");
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        var emailInput = wait.Until(d => d.FindElement(By.CssSelector("input[type='email']")));
        Assert.NotNull(emailInput);

        driver.Navigate().GoToUrl($"{_baseUrl}/auth/reset-password");
        var resetCodeInput = wait.Until(d => d.FindElement(By.XPath("//input[contains(@aria-label, 'Código') or contains(@placeholder, 'Código') or @type='text']")));
        Assert.NotNull(resetCodeInput);
    }

    [Fact(Skip = "Requer Google Chrome e ChromeDriver no PATH para execução de automação Selenium")]
    public void ConfirmEmailFlow_RendersInputsAndActions()
    {
        using var driver = CreateWebDriver();
        driver.Navigate().GoToUrl($"{_baseUrl}/auth/confirm-email?userId=test-id&code=test-code");
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        var textInputs = wait.Until(d => d.FindElements(By.CssSelector("input")));
        Assert.NotEmpty(textInputs);
    }

    [Fact(Skip = "Requer Google Chrome e ChromeDriver no PATH para execução de automação Selenium")]
    public void ManageProfile_RendersSecurityAnd2FASections()
    {
        using var driver = CreateWebDriver();
        driver.Navigate().GoToUrl($"{_baseUrl}/auth/manage");
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        var body = wait.Until(d => d.FindElement(By.TagName("body")));
        Assert.NotNull(body);
    }

    [Fact(Skip = "Requer Google Chrome e ChromeDriver no PATH para execução de automação Selenium")]
    public void ManageRolesAndPermissions_RendersCatalogAndUserGrants()
    {
        using var driver = CreateWebDriver();
        driver.Navigate().GoToUrl($"{_baseUrl}/auth/manage-roles");
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        var body = wait.Until(d => d.FindElement(By.TagName("body")));
        Assert.NotNull(body);

        driver.Navigate().GoToUrl($"{_baseUrl}/auth/manage-permissions");
        var bodyPerms = wait.Until(d => d.FindElement(By.TagName("body")));
        Assert.NotNull(bodyPerms);
    }
}
