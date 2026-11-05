using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using HF.Shared;
using HF.Shared.Auth;
using HF.Shared.Enrollments;
using HF.Shared.PageModels.Enrollments;
using HF.Shared.Pages.Enrollments;
using Xunit;

namespace HF.UITests.Shared;

public class SharedEnrollmentsRouteAndComponentDiscoveryTests
{
    [Fact]
    public void SharedAssembly_ExportsAllEnrollmentsPages_WithCorrectRoutes()
    {
        var sharedAssembly = typeof(EnrollmentList).Assembly;

        var routeEntries = sharedAssembly.ExportedTypes
            .Where(t => typeof(IComponent).IsAssignableFrom(t))
            .SelectMany(t => t.GetCustomAttributes<RouteAttribute>().Select(attr => new { Template = attr.Template, Component = t }))
            .ToList();

        Assert.Contains(routeEntries, r => r.Template == "/enrollments" && r.Component == typeof(EnrollmentList));
        Assert.Contains(routeEntries, r => r.Template == "/enrollments/create" && r.Component == typeof(EnrollmentCreate));
        Assert.Contains(routeEntries, r => r.Template == "/enrollments/{Id}" && r.Component == typeof(EnrollmentDetails));
        Assert.Contains(routeEntries, r => r.Template == "/enrollments/{Id}/edit" && r.Component == typeof(EnrollmentEdit));
    }

    [Fact]
    public void EnrollmentPages_AreExplicitlyDecoratedWithAuthorizeAttribute()
    {
        var pages = new[]
        {
            (Component: typeof(EnrollmentList), ExpectedPolicy: AppPolicies.EnrollmentsRead),
            (Component: typeof(EnrollmentCreate), ExpectedPolicy: AppPolicies.EnrollmentsCreate),
            (Component: typeof(EnrollmentDetails), ExpectedPolicy: AppPolicies.EnrollmentsRead),
            (Component: typeof(EnrollmentEdit), ExpectedPolicy: AppPolicies.EnrollmentsUpdate)
        };

        foreach (var (component, expectedPolicy) in pages)
        {
            var authAttribute = component.GetCustomAttribute<AuthorizeAttribute>(inherit: true);
            Assert.NotNull(authAttribute);
            Assert.Equal(expectedPolicy, authAttribute.Policy);
        }
    }

    [Fact]
    public void AddPageModels_RegistersAllEnrollmentsPageModels_AndEnrollmentService_AsScoped()
    {
        var services = new ServiceCollection();

        // Calling AddPageModels() as used in HF.Web (Program.cs) and HF.Mobile (MauiProgram.cs)
        services.AddPageModels();

        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        // Verify PageModels resolution
        var listModel = sp.GetService<EnrollmentListPageModel>();
        Assert.NotNull(listModel);
        Assert.Equal("Matrículas", listModel.Title);

        var createModel = sp.GetService<EnrollmentCreatePageModel>();
        Assert.NotNull(createModel);
        Assert.Equal("Nova Matrícula", createModel.Title);

        var detailsModel = sp.GetService<EnrollmentDetailsPageModel>();
        Assert.NotNull(detailsModel);
        Assert.Equal("Detalhes da Matrícula", detailsModel.Title);

        var editModel = sp.GetService<EnrollmentEditPageModel>();
        Assert.NotNull(editModel);
        Assert.Equal("Editar Matrícula", editModel.Title);

        // Verify Service resolution
        var enrollmentService = sp.GetService<IEnrollmentService>();
        Assert.NotNull(enrollmentService);
        Assert.IsType<EnrollmentService>(enrollmentService);
    }
}

