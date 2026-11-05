using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using HF.Shared;
using HF.Shared.PageModels.Teachers;
using HF.Shared.Pages.Teachers;
using HF.Shared.Teachers;
using Xunit;

namespace HF.UITests.Shared;

public class SharedTeachersRouteAndComponentDiscoveryTests
{
    [Fact]
    public void SharedAssembly_ExportsAllTeacherPages_WithCorrectRoutes()
    {
        var sharedAssembly = typeof(TeacherList).Assembly;

        var routeEntries = sharedAssembly.ExportedTypes
            .Where(t => typeof(IComponent).IsAssignableFrom(t))
            .SelectMany(t => t.GetCustomAttributes<RouteAttribute>().Select(attr => new { Template = attr.Template, Component = t }))
            .ToList();

        Assert.Contains(routeEntries, r => r.Template == "/teachers" && r.Component == typeof(TeacherList));
        Assert.Contains(routeEntries, r => r.Template == "/teachers/create" && r.Component == typeof(TeacherCreate));
        Assert.Contains(routeEntries, r => r.Template == "/teachers/{Id}" && r.Component == typeof(TeacherDetails));
        Assert.Contains(routeEntries, r => r.Template == "/teachers/{Id}/edit" && r.Component == typeof(TeacherEdit));
    }

    [Fact]
    public void TeacherPages_AreExplicitlyDecoratedWithAuthorizeAttribute()
    {
        var teacherPages = new[]
        {
            typeof(TeacherList),
            typeof(TeacherCreate),
            typeof(TeacherDetails),
            typeof(TeacherEdit)
        };

        foreach (var pageType in teacherPages)
        {
            var authAttribute = pageType.GetCustomAttribute<AuthorizeAttribute>(inherit: true);
            Assert.True(authAttribute != null, $"Page '{pageType.Name}' must have [Authorize] attribute to protect forms and actions from anonymous access.");
        }
    }

    [Fact]
    public void AddPageModels_RegistersAllTeacherPageModels_AndService_AsScoped()
    {
        var services = new ServiceCollection();

        services.AddPageModels();

        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        var listModel = sp.GetService<TeacherListPageModel>();
        Assert.NotNull(listModel);
        Assert.Equal("Professores", listModel.Title);

        var createModel = sp.GetService<TeacherCreatePageModel>();
        Assert.NotNull(createModel);
        Assert.Equal("Cadastrar Professor", createModel.Title);

        var detailsModel = sp.GetService<TeacherDetailsPageModel>();
        Assert.NotNull(detailsModel);

        var editModel = sp.GetService<TeacherEditPageModel>();
        Assert.NotNull(editModel);

        var teacherService = sp.GetService<ITeacherService>();
        Assert.NotNull(teacherService);
    }
}

