using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using HF.Shared;
using HF.Shared.Auth;
using HF.Shared.PageModels.Auth;
using HF.Shared.Pages.Auth;
using Xunit;

namespace HF.UITests.Shared;

public class SharedAuthRouteAndComponentDiscoveryTests
{
    [Fact]
    public void SharedAssembly_ExportsAllAuthPages_WithCorrectRoutes()
    {
        var sharedAssembly = typeof(Login).Assembly;

        var routeEntries = sharedAssembly.ExportedTypes
            .Where(t => typeof(IComponent).IsAssignableFrom(t))
            .SelectMany(t => t.GetCustomAttributes<RouteAttribute>().Select(attr => new { Template = attr.Template, Component = t }))
            .ToList();

        Assert.Contains(routeEntries, r => r.Template == "/auth/login" && r.Component == typeof(Login));
        Assert.Contains(routeEntries, r => r.Template == "/auth/register" && r.Component == typeof(Register));
        Assert.Contains(routeEntries, r => r.Template == "/auth/forgot-password" && r.Component == typeof(ForgotPassword));
        Assert.Contains(routeEntries, r => r.Template == "/auth/reset-password" && r.Component == typeof(ResetPassword));
        Assert.Contains(routeEntries, r => r.Template == "/auth/confirm-email" && r.Component == typeof(ConfirmEmail));
        Assert.Contains(routeEntries, r => r.Template == "/auth/manage" && r.Component == typeof(ManageProfile));
        Assert.Contains(routeEntries, r => r.Template == "/auth/manage-roles" && r.Component == typeof(ManageRoles));
        Assert.Contains(routeEntries, r => r.Template == "/auth/manage-permissions" && r.Component == typeof(ManagePermissions));
    }

    [Fact]
    public void AddPageModels_RegistersAllAuthPageModels_AsScopedServices()
    {
        var services = new ServiceCollection();

        // Calling AddPageModels() as used in HF.Web (Program.cs) and HF.Mobile (MauiProgram.cs)
        services.AddPageModels();

        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        var loginModel = sp.GetService<LoginPageModel>();
        Assert.NotNull(loginModel);
        Assert.Equal("Entrar no Horizon", loginModel.Title);

        var registerModel = sp.GetService<RegisterPageModel>();
        Assert.NotNull(registerModel);

        var forgotPasswordModel = sp.GetService<ForgotPasswordPageModel>();
        Assert.NotNull(forgotPasswordModel);

        var resetPasswordModel = sp.GetService<ResetPasswordPageModel>();
        Assert.NotNull(resetPasswordModel);

        var confirmEmailModel = sp.GetService<ConfirmEmailPageModel>();
        Assert.NotNull(confirmEmailModel);

        var manageProfileModel = sp.GetService<ManageProfilePageModel>();
        Assert.NotNull(manageProfileModel);

        var manageRolesModel = sp.GetService<ManageRolesPageModel>();
        Assert.NotNull(manageRolesModel);

        var managePermissionsModel = sp.GetService<ManagePermissionsPageModel>();
        Assert.NotNull(managePermissionsModel);

        var authService = sp.GetService<IAuthService>();
        Assert.NotNull(authService);

        var tokenStore = sp.GetService<ITokenStore>();
        Assert.NotNull(tokenStore);
    }
}

