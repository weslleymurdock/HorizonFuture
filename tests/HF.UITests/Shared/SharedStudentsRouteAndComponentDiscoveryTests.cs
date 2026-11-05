using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using HF.Shared;
using HF.Shared.Students;
using HF.Shared.PageModels.Students;
using HF.Shared.Pages.Students;
using Xunit;

namespace HF.UITests.Shared;

public class SharedStudentsRouteAndComponentDiscoveryTests
{
    [Fact]
    public void SharedAssembly_ExportsAllStudentsPages_WithCorrectRoutes()
    {
        var sharedAssembly = typeof(StudentList).Assembly;

        var routeEntries = sharedAssembly.ExportedTypes
            .Where(t => typeof(IComponent).IsAssignableFrom(t))
            .SelectMany(t => t.GetCustomAttributes<RouteAttribute>().Select(attr => new { Template = attr.Template, Component = t }))
            .ToList();

        Assert.Contains(routeEntries, r => r.Template == "/students" && r.Component == typeof(StudentList));
        Assert.Contains(routeEntries, r => r.Template == "/students/create" && r.Component == typeof(StudentCreate));
        Assert.Contains(routeEntries, r => r.Template == "/students/{Id}" && r.Component == typeof(StudentDetails));
        Assert.Contains(routeEntries, r => r.Template == "/students/{Id}/edit" && r.Component == typeof(StudentEdit));
    }

    [Fact]
    public void AddPageModels_RegistersAllStudentsPageModels_AndStudentService_AsScoped()
    {
        var services = new ServiceCollection();

        // Calling AddPageModels() as used in HF.Web (Program.cs) and HF.Mobile (MauiProgram.cs)
        services.AddPageModels();

        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        // Verify PageModels resolution
        var listModel = sp.GetService<StudentListPageModel>();
        Assert.NotNull(listModel);
        Assert.Equal("Alunos", listModel.Title);

        var createModel = sp.GetService<StudentCreatePageModel>();
        Assert.NotNull(createModel);
        Assert.Equal("Cadastrar Aluno", createModel.Title);

        var detailsModel = sp.GetService<StudentDetailsPageModel>();
        Assert.NotNull(detailsModel);
        Assert.Equal("Detalhes do Aluno", detailsModel.Title);

        var editModel = sp.GetService<StudentEditPageModel>();
        Assert.NotNull(editModel);
        Assert.Equal("Editar Aluno", editModel.Title);

        // Verify Service resolution
        var studentService = sp.GetService<IStudentService>();
        Assert.NotNull(studentService);
        Assert.IsType<StudentService>(studentService);
    }
}

