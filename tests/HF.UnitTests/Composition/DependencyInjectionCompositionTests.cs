using FluentValidation;
using HF.Application.AssetsLiabilities.Abstract;
using HF.Application.Attendances.Abstract;
using HF.Application.Auth.Abstract;
using HF.Application.Auth.Validators;
using HF.Application.ClassSessions.Abstract;
using HF.Application.Collaborators.Abstract;
using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Courses.Abstract;
using HF.Application.Directors.Abstract;
using HF.Application.Enrollments.Abstract;
using HF.Application.Managers.Abstract;
using HF.Application.Students.Abstract;
using HF.Application.TeacherAssignments.Abstract;
using HF.Application.Teachers.Abstract;
using HF.Application.Volunteers.Abstract;
using HF.Composition.Extensions;
using HF.Domain.AssetsLiabilities;
using HF.Domain.Attendances;
using HF.Domain.ClassSessions;
using HF.Domain.Collaborators;
using HF.Domain.Courses;
using HF.Domain.Directors;
using HF.Domain.Enrollments;
using HF.Domain.Managers;
using HF.Domain.Students;
using HF.Domain.TeacherAssignments;
using HF.Domain.Teachers;
using HF.Domain.Volunteers;
using HF.Infrastructure.Auth.Models;
using HF.Infrastructure.Auth.Requirements;
using HF.Infrastructure.Persistence;
using HF.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace HF.Application.Tests.Composition;

public class DependencyInjectionCompositionTests
{
    private static WebApplication CreateTestWebApplication()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:PostgresConnection"] = "Host=localhost;Database=testdb;Username=postgres;Password=postgres",
            ["Jwt:Key"] = "HorizonFuture-Development-Secret-Key-Must-Be-At-Least-32-Bytes-Long-123456",
            ["Jwt:Issuer"] = "HorizonFuture",
            ["Jwt:Audience"] = "HorizonFuture",
            ["ADMIN_EMAIL"] = "admin@horizonfuture.org",
            ["ADMIN_USERNAME"] = "admin",
            ["ADMIN_PASSWORD"] = "Admin1234!"
        });

        builder.BuildHF(builder.Environment);
        builder.Services.AddControllers().AddControllersAsServices();

        return builder.Build();
    }

    [Fact]
    public void BuildWebApplication_WithValidateOnBuildAndValidateScopes_Succeeds()
    {
        using var app = CreateTestWebApplication();
        Assert.NotNull(app);
    }

    [Fact]
    public void ResolveServices_FromRealComposition_ResolvesAllApplicationAndInfrastructureServices()
    {
        using var app = CreateTestWebApplication();
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        // Application domain services
        Assert.NotNull(sp.GetRequiredService<IStudentService>());
        Assert.NotNull(sp.GetRequiredService<ITeacherService>());
        Assert.NotNull(sp.GetRequiredService<IManagerService>());
        Assert.NotNull(sp.GetRequiredService<IDirectorService>());
        Assert.NotNull(sp.GetRequiredService<ICollaboratorService>());
        Assert.NotNull(sp.GetRequiredService<IVolunteerService>());
        Assert.NotNull(sp.GetRequiredService<ICourseService>());
        Assert.NotNull(sp.GetRequiredService<ISubjectService>());
        Assert.NotNull(sp.GetRequiredService<IAssetService>());
        Assert.NotNull(sp.GetRequiredService<ILiabilityService>());
        Assert.NotNull(sp.GetRequiredService<ITeacherAssignmentService>());
        Assert.NotNull(sp.GetRequiredService<IEnrollmentService>());
        Assert.NotNull(sp.GetRequiredService<IClassSessionService>());
        Assert.NotNull(sp.GetRequiredService<IAttendanceService>());

        // Auth & Identity services
        Assert.NotNull(sp.GetRequiredService<IIdentityService>());
        Assert.NotNull(sp.GetRequiredService<IIdentityAdminService>());
        Assert.NotNull(sp.GetRequiredService<IUserPermissionEvaluator>());
        Assert.NotNull(sp.GetRequiredService<IJwtTokenService>());
        Assert.NotNull(sp.GetRequiredService<IIdentityEmailSender>());
        Assert.NotNull(sp.GetRequiredService<IRevokedTokenStore>());

        // Identity Core Managers
        Assert.NotNull(sp.GetRequiredService<UserManager<User>>());
        Assert.NotNull(sp.GetRequiredService<RoleManager<Role>>());
        Assert.NotNull(sp.GetRequiredService<SignInManager<User>>());

        // Authorization Handler
        var authHandlers = sp.GetServices<IAuthorizationHandler>();
        Assert.Contains(authHandlers, h => h is PermissionAuthorizationHandler);

        // Persistence & Generic Repositories
        Assert.NotNull(sp.GetRequiredService<HFDbContext>());
        Assert.NotNull(sp.GetRequiredService<IUnitOfWork>());
        Assert.NotNull(sp.GetRequiredService<IRepository<Student>>());
        Assert.NotNull(sp.GetRequiredService<IRepository<Teacher>>());
        Assert.NotNull(sp.GetRequiredService<IRepository<Course>>());
        Assert.NotNull(sp.GetRequiredService<IRepository<Subject>>());
        Assert.NotNull(sp.GetRequiredService<IRepository<Enrollment>>());
        Assert.NotNull(sp.GetRequiredService<IRepository<ClassSession>>());
        Assert.NotNull(sp.GetRequiredService<IRepository<Attendance>>());
        Assert.NotNull(sp.GetRequiredService<IRepository<Asset>>());
        Assert.NotNull(sp.GetRequiredService<IRepository<Liability>>());
        Assert.NotNull(sp.GetRequiredService<IRepository<Volunteer>>());
        Assert.NotNull(sp.GetRequiredService<IRepository<Collaborator>>());
        Assert.NotNull(sp.GetRequiredService<IRepository<Manager>>());
        Assert.NotNull(sp.GetRequiredService<IRepository<Director>>());
        Assert.NotNull(sp.GetRequiredService<IRepository<TeacherAssignment>>());

        // Hosted Services
        var hostedServices = sp.GetServices<IHostedService>();
        Assert.Contains(hostedServices, s => s is DatabaseSeedHostedService);

        // Health Checks
        var healthCheckService = sp.GetRequiredService<HealthCheckService>();
        Assert.NotNull(healthCheckService);
    }

    [Fact]
    public void ResolveControllers_FromAPI_ActivatesAllControllerTypesWithoutError()
    {
        using var app = CreateTestWebApplication();
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        var controllerAssembly = typeof(HF.API.Controllers.v1.AuthController).Assembly;
        var controllerTypes = controllerAssembly
            .GetTypes()
            .Where(t => !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t))
            .ToList();

        Assert.NotEmpty(controllerTypes);
        foreach (var controllerType in controllerTypes)
        {
            var controller = ActivatorUtilities.CreateInstance(sp, controllerType);
            Assert.NotNull(controller);
        }
    }

    [Fact]
    public void ResolveHandlers_FromApplication_ResolvesAllHandlers()
    {
        using var app = CreateTestWebApplication();
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        var handlerTypes = new[]
        {
            typeof(HF.Application.Auth.Handlers.AuthHandlers),
            typeof(HF.Application.Students.Handlers.StudentHandlers),
            typeof(HF.Application.Teachers.Handlers.TeacherHandlers),
            typeof(HF.Application.Managers.Handlers.ManagerHandlers),
            typeof(HF.Application.Directors.Handlers.DirectorHandlers),
            typeof(HF.Application.Collaborators.Handlers.CollaboratorHandlers),
            typeof(HF.Application.Volunteers.Handlers.VolunteerHandlers),
            typeof(HF.Application.Courses.Handlers.CourseHandlers),
            typeof(HF.Application.Courses.Handlers.SubjectHandlers),
            typeof(HF.Application.AssetsLiabilities.Handlers.AssetHandlers),
            typeof(HF.Application.AssetsLiabilities.Handlers.LiabilityHandlers),
            typeof(HF.Application.TeacherAssignments.Handlers.TeacherAssignmentHandlers),
            typeof(HF.Application.Enrollments.Handlers.EnrollmentHandlers),
            typeof(HF.Application.ClassSessions.Handlers.ClassSessionHandlers),
            typeof(HF.Application.Attendances.Handlers.AttendanceHandlers)
        };

        foreach (var handlerType in handlerTypes)
        {
            var handler = sp.GetRequiredService(handlerType);
            Assert.NotNull(handler);
        }
    }

    [Fact]
    public void ResolveValidators_FromApplication_ResolvesAllRegisteredValidators()
    {
        using var app = CreateTestWebApplication();
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        var validatorTypes = typeof(RegisterRequestValidator).Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)))
            .ToList();

        Assert.NotEmpty(validatorTypes);
        foreach (var validatorType in validatorTypes)
        {
            var validator = sp.GetRequiredService(validatorType);
            Assert.NotNull(validator);
        }
    }

    [Fact]
    public void StartupConfigurationValidation_MissingPostgresConnectionString_ThrowsClearException()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build(); // No Postgres connection string

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            services.AddDatabase(configuration);
        });

        Assert.Contains("ConnectionStrings__PostgresConnection", ex.Message);
        Assert.DoesNotContain("password", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StartupConfigurationValidation_ShortJwtKey_ThrowsClearException()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "too-short-key" // Less than 32 chars
            })
            .Build();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            services.AddAuth(configuration);
        });

        Assert.Contains("Jwt:Key", ex.Message);
        Assert.Contains("32", ex.Message);
    }

    [Fact]
    public async Task HealthChecks_WhenDatabaseUnreachable_ExecutesAllChecksAndReportsDatabaseUnhealthy()
    {
        using var app = CreateTestWebApplication();
        using var scope = app.Services.CreateScope();
        var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var report = await healthCheckService.CheckHealthAsync(cts.Token);
        Assert.NotNull(report);
        Assert.Contains("ApiCheck", report.Entries.Keys);
        Assert.Contains("HostInfoCheck", report.Entries.Keys);
        Assert.Contains("DatabaseCheck", report.Entries.Keys);
        Assert.Contains("DatabaseUsageCheck", report.Entries.Keys);

        Assert.Equal(HealthStatus.Healthy, report.Entries["ApiCheck"].Status);
        Assert.Equal(HealthStatus.Healthy, report.Entries["HostInfoCheck"].Status);
        Assert.Equal(HealthStatus.Unhealthy, report.Entries["DatabaseCheck"].Status);
    }
}
