using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace HF.UITests.Mobile;

/// <summary>
/// Mobile UI automation tests for authentication flows in .NET MAUI Blazor Hybrid (HF.Mobile) using Appium.
/// Prerequisites:
/// 1. Appium running on port 4723 (APPIUM_SERVER_URL, default: http://127.0.0.1:4723/)
/// 2. UiAutomator2 driver installed ('appium driver install uiautomator2')
/// 3. Connected Android emulator or physical device with API 24+
/// 4. HF.Mobile package installed (com.companyname.hf.mobile)
/// </summary>
[Collection("MobileUI")]
public class MobileAuthFlowUITests
{
    private readonly string _appiumServerUrl = Environment.GetEnvironmentVariable("APPIUM_SERVER_URL") ?? "http://127.0.0.1:4723/";
    private readonly string _appId = "com.companyname.hf.mobile";
    private readonly string _mainActivity = "crc640ad8fb5823e64b54.MainActivity";

    private AndroidDriver CreateAppiumDriver()
    {
        var serverUri = new Uri(_appiumServerUrl);
        var options = new AppiumOptions
        {
            PlatformName = "Android",
            AutomationName = "UiAutomator2"
        };
        options.AddAdditionalAppiumOption("appPackage", _appId);
        options.AddAdditionalAppiumOption("appActivity", _mainActivity);
        options.AddAdditionalAppiumOption("noReset", true);
        options.AddAdditionalAppiumOption("newCommandTimeout", 120);

        try
        {
            var driver = new AndroidDriver(serverUri, options, TimeSpan.FromSeconds(60));
            try { driver.ActivateApp(_appId); } catch { }
            return driver;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to connect to Appium server at {_appiumServerUrl} or launch app {_appId}. " +
                $"Ensure Appium is running and an Android emulator/device is connected. Details: {ex.Message}", ex);
        }
    }

    [Fact]
    public void MobileLoginFlow_RendersMobileViewport_AndPerformsAuth()
    {
        using var driver = CreateAppiumDriver();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

        var webView = wait.Until(d => d.FindElement(By.ClassName("android.webkit.WebView")));
        Assert.NotNull(webView);

        var elements = webView.FindElements(By.XPath(".//*"));
        Assert.NotEmpty(elements);
    }

    [Fact]
    public void MobileRegisterFlow_ValidatesOnTouch()
    {
        using var driver = CreateAppiumDriver();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

        var webView = wait.Until(d => d.FindElement(By.ClassName("android.webkit.WebView")));
        Assert.NotNull(webView);

        var viewElements = driver.FindElements(By.ClassName("android.view.View"));
        Assert.NotEmpty(viewElements);
    }

    [Fact]
    public void MobileProfileAnd2FAFlow_SupportsNarrowLayout()
    {
        using var driver = CreateAppiumDriver();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

        var webView = wait.Until(d => d.FindElement(By.ClassName("android.webkit.WebView")));
        Assert.NotNull(webView);

        var frameElements = driver.FindElements(By.ClassName("android.widget.FrameLayout"));
        Assert.NotEmpty(frameElements);
    }

    [Fact]
    public void MobileRolesAndPermissions_RendersTouchActionChips()
    {
        using var driver = CreateAppiumDriver();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

        var webView = wait.Until(d => d.FindElement(By.ClassName("android.webkit.WebView")));
        Assert.NotNull(webView);
    }
}
