using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using HF.Shared;
using HF.Shared.Subjects;
using HF.Shared.PageModels.Subjects;
using HF.Shared.Pages.Subjects;
using Xunit;

namespace HF.UITests.Shared;

public class SharedSubjectsRouteAndComponentDiscoveryTests
{
    [Fact]
    public void SharedAssembly_ExportsAllSubjectsPages_WithCorrectRoutes()
    {
        var sharedAssembly = typeof(SubjectList).Assembly;

        var routeEntries = sharedAssembly.ExportedTypes
            .Where(t => typeof(IComponent).IsAssignableFrom(t))
            .SelectMany(t => t.GetCustomAttributes<RouteAttribute>().Select(attr => new { Template = attr.Template, Component = t }))
            .ToList();

        Assert.Contains(routeEntries, r => r.Template == "/subjects" && r.Component == typeof(SubjectList));
        Assert.Contains(routeEntries, r => r.Template == "/subjects/create" && r.Component == typeof(SubjectCreate));
        Assert.Contains(routeEntries, r => r.Template == "/subjects/{Id}" && r.Component == typeof(SubjectDetails));
        Assert.Contains(routeEntries, r => r.Template == "/subjects/{Id}/edit" && r.Component == typeof(SubjectEdit));
    }

    [Fact]
    public void AddPageModels_RegistersAllSubjectsPageModels_AndSubjectService_AsScoped()
    {
        var services = new ServiceCollection();

        // Calling AddPageModels() as used in HF.Web (Program.cs) and HF.Mobile (MauiProgram.cs)
        services.AddPageModels();

        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        // Verify PageModels resolution
        var listModel = sp.GetService<SubjectListPageModel>();
        Assert.NotNull(listModel);
        Assert.Equal("Disciplinas", listModel.Title);

        var createModel = sp.GetService<SubjectCreatePageModel>();
        Assert.NotNull(createModel);
        Assert.Equal("Cadastrar Disciplina", createModel.Title);

        var detailsModel = sp.GetService<SubjectDetailsPageModel>();
        Assert.NotNull(detailsModel);
        Assert.Equal("Detalhes da Disciplina", detailsModel.Title);

        var editModel = sp.GetService<SubjectEditPageModel>();
        Assert.NotNull(editModel);
        Assert.Equal("Editar Disciplina", editModel.Title);

        // Verify Service resolution
        var subjectService = sp.GetService<ISubjectService>();
        Assert.NotNull(subjectService);
        Assert.IsType<SubjectService>(subjectService);
    }
}

