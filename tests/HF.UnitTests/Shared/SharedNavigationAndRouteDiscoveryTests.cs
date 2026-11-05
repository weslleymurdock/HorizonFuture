using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using HF.Shared.Layout;
using HF.Shared.PageModels;
using HF.Shared.Pages;
using Xunit;

namespace HF.UnitTests.Shared;

public class SharedNavigationAndRouteDiscoveryTests
{
    [Fact]
    public void SharedAssembly_ContainsExpectedRclPages()
    {
        var sharedAssembly = typeof(MainLayout).Assembly;

        var routeEntries = sharedAssembly.ExportedTypes
            .Where(t => typeof(IComponent).IsAssignableFrom(t))
            .SelectMany(t => t.GetCustomAttributes<RouteAttribute>().Select(attr => new { Template = attr.Template, Component = t }))
            .ToList();

        Assert.Contains(routeEntries, r => r.Template == "/" && r.Component == typeof(Home)); 
        Assert.Contains(routeEntries, r => r.Template == "/not-found" && r.Component == typeof(NotFound));
    }

    [Fact]
    public void RouteDiscovery_FindsMatchingPage_ForSpecifiedRoutes()
    {
        var sharedAssembly = typeof(MainLayout).Assembly;
        var routes = sharedAssembly.ExportedTypes
            .Where(t => typeof(IComponent).IsAssignableFrom(t))
            .SelectMany(t => t.GetCustomAttributes<RouteAttribute>().Select(a => new { a.Template, ComponentType = t }))
            .ToDictionary(r => r.Template, r => r.ComponentType);

        // Verify root route
        Assert.True(routes.TryGetValue("/", out var homeType));
        Assert.Equal(typeof(Home), homeType);

 
        // Verify not-found route
        Assert.True(routes.TryGetValue("/not-found", out var notFoundType));
        Assert.Equal(typeof(NotFound), notFoundType);

        // Verify unknown route falls back to not found
        string unknownRoute = "/unknown-page";
        bool isKnown = routes.ContainsKey(unknownRoute);
        Assert.False(isKnown);
    }

    [Fact]
    public void NotFoundPage_UsesMainLayout()
    {
        var layoutAttr = typeof(NotFound).GetCustomAttribute<LayoutAttribute>();
        Assert.NotNull(layoutAttr);
        Assert.Equal(typeof(MainLayout), layoutAttr.LayoutType);
    }

    [Fact]
    public void DependencyInjection_CanResolveSharedPageModels()
    {
        var services = new ServiceCollection();
        services.AddScoped<PageModelBase>();
        services.AddScoped<HomePageModel>();

        var provider = services.BuildServiceProvider();
        var homePageModel = provider.GetService<HomePageModel>();

        Assert.NotNull(homePageModel);
        Assert.Equal("Home", homePageModel.Title);
        Assert.False(homePageModel.IsBusy);
    }
}

