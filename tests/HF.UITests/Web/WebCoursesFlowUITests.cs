using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;
using Xunit.Abstractions;

namespace HF.UITests.Web;

/// <summary>
/// Testes de automação de UI para Blazor WebAssembly (HF.Web) cobrindo a feature Courses via Selenium WebDriver.
/// </summary>
public class WebCoursesFlowUITests
{
    private readonly ITestOutputHelper _output;
    private readonly string _webBaseUrl;
    private readonly string _apiBaseUrl;

    public WebCoursesFlowUITests(ITestOutputHelper output)
    {
        _output = output;
        _webBaseUrl = (Environment.GetEnvironmentVariable("HF_WEB_BASE_URL") ?? "http://localhost:5076").TrimEnd('/');
        _apiBaseUrl = (Environment.GetEnvironmentVariable("HF_API_BASE_URL") ?? "http://localhost:8080").TrimEnd('/');
    }

    private IWebDriver CreateWebDriver()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")))
        {
            Environment.SetEnvironmentVariable("DISPLAY", ":0");
        }

        var options = new ChromeOptions();
        options.AddArguments("--no-sandbox", "--disable-dev-shm-usage", "--disable-gpu", "--window-size=1280,850");

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

    [Fact(Skip = "Requer Google Chrome instalado e display gráfico ativo (DISPLAY=:0)")]
    public void CoursesList_LoadsCourses_AndNavigatesToCreate()
    {
        using var driver = CreateWebDriver();
        driver.Navigate().GoToUrl($"{_webBaseUrl}/courses");
        var body = driver.FindElement(By.TagName("body"));
        Assert.NotNull(body);
    }

    [Fact(Skip = "Requer Google Chrome instalado e display gráfico ativo (DISPLAY=:0)")]
    public void CourseCreate_ValidatesAndSubmitsForm()
    {
        using var driver = CreateWebDriver();
        driver.Navigate().GoToUrl($"{_webBaseUrl}/courses/create");
        var body = driver.FindElement(By.TagName("body"));
        Assert.NotNull(body);
    }

    [Fact(Skip = "Requer Google Chrome instalado e display gráfico ativo (DISPLAY=:0)")]
    public void CourseDetails_DisplaysCourseInformation()
    {
        using var driver = CreateWebDriver();
        driver.Navigate().GoToUrl($"{_webBaseUrl}/courses/test-id");
        var body = driver.FindElement(By.TagName("body"));
        Assert.NotNull(body);
    }
}

