using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using HF.Shared;
using HF.Shared.Courses;
using HF.Shared.PageModels.Courses;
using HF.Shared.Pages.Courses;
using Xunit;

namespace HF.UITests.Shared;

public class SharedCoursesRouteAndComponentDiscoveryTests
{
    [Fact]
    public void SharedAssembly_ExportsAllCoursesPages_WithCorrectRoutes()
    {
        var sharedAssembly = typeof(CourseList).Assembly;

        var routeEntries = sharedAssembly.ExportedTypes
            .Where(t => typeof(IComponent).IsAssignableFrom(t))
            .SelectMany(t => t.GetCustomAttributes<RouteAttribute>().Select(attr => new { Template = attr.Template, Component = t }))
            .ToList();

        Assert.Contains(routeEntries, r => r.Template == "/courses" && r.Component == typeof(CourseList));
        Assert.Contains(routeEntries, r => r.Template == "/courses/create" && r.Component == typeof(CourseCreate));
        Assert.Contains(routeEntries, r => r.Template == "/courses/{Id}" && r.Component == typeof(CourseDetails));
        Assert.Contains(routeEntries, r => r.Template == "/courses/{Id}/edit" && r.Component == typeof(CourseEdit));
    }

    [Fact]
    public void AddPageModels_RegistersAllCoursesPageModels_AndCourseService_AsScoped()
    {
        var services = new ServiceCollection();

        // Calling AddPageModels() as used in HF.Web (Program.cs) and HF.Mobile (MauiProgram.cs)
        services.AddPageModels();

        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        // Verify PageModels resolution
        var listModel = sp.GetService<CourseListPageModel>();
        Assert.NotNull(listModel);
        Assert.Equal("Cursos", listModel.Title);

        var createModel = sp.GetService<CourseCreatePageModel>();
        Assert.NotNull(createModel);
        Assert.Equal("Cadastrar Curso", createModel.Title);

        var detailsModel = sp.GetService<CourseDetailsPageModel>();
        Assert.NotNull(detailsModel);
        Assert.Equal("Detalhes do Curso", detailsModel.Title);

        var editModel = sp.GetService<CourseEditPageModel>();
        Assert.NotNull(editModel);
        Assert.Equal("Editar Curso", editModel.Title);

        // Verify Service resolution
        var courseService = sp.GetService<ICourseService>();
        Assert.NotNull(courseService);
        Assert.IsType<CourseService>(courseService);
    }
}

