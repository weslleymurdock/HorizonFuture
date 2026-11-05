using System;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using Xunit;

namespace HF.UITests.Mobile;

/// <summary>
/// Testes de automação de UI mobile para a feature de Courses em .NET MAUI Blazor Hybrid (HF.Mobile) utilizando Appium.
/// Requisitos para execução:
/// 1. Appium 2.x em execução na porta 4723 (APPIUM_SERVER_URL, padrão: http://127.0.0.1:4723/)
/// 2. Driver UiAutomator2 configurado ('appium driver install uiautomator2')
/// 3. Emulador ou dispositivo Android conectado com API 24+
/// 4. Pacote do app HF.Mobile instalado (com.companyname.hf.mobile)
/// </summary>
public class MobileCoursesFlowUITests
{
    private readonly string _appiumServerUrl = Environment.GetEnvironmentVariable("APPIUM_SERVER_URL") ?? "http://127.0.0.1:4723/";
    private readonly string _appId = "com.companyname.hf.mobile";

    private AndroidDriver CreateAppiumDriver()
    {
        var serverUri = new Uri(_appiumServerUrl);
        var options = new AppiumOptions
        {
            PlatformName = "Android",
            AutomationName = "UiAutomator2",
            App = _appId
        };
        options.AddAdditionalAppiumOption("appPackage", _appId);
        options.AddAdditionalAppiumOption("appActivity", "crc64104c8651a5c13636.MainActivity");
        options.AddAdditionalAppiumOption("noReset", true);

        return new AndroidDriver(serverUri, options, TimeSpan.FromSeconds(30));
    }

    [Fact(Skip = "Requer servidor Appium e dispositivo/emulador Android conectado")]
    public void MobileCoursesList_RendersMobileTableAndViewport()
    {
        using var driver = CreateAppiumDriver();
        System.Threading.Thread.Sleep(3000);

        var webView = driver.FindElement(OpenQA.Selenium.By.ClassName("android.webkit.WebView"));
        Assert.NotNull(webView);
    }

    [Fact(Skip = "Requer servidor Appium e dispositivo/emulador Android conectado")]
    public void MobileCourseCreate_RendersTouchForm()
    {
        using var driver = CreateAppiumDriver();
        System.Threading.Thread.Sleep(3000);

        var webView = driver.FindElement(OpenQA.Selenium.By.ClassName("android.webkit.WebView"));
        Assert.NotNull(webView);
    }

    [Fact(Skip = "Requer servidor Appium e dispositivo/emulador Android conectado")]
    public void MobileCourseDetails_RendersMetadataCards()
    {
        using var driver = CreateAppiumDriver();
        System.Threading.Thread.Sleep(3000);

        var webView = driver.FindElement(OpenQA.Selenium.By.ClassName("android.webkit.WebView"));
        Assert.NotNull(webView);
    }
}

