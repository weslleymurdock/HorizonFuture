using FluentValidation;
using HF.Application.AssetsLiabilities.Abstract;
using HF.Application.AssetsLiabilities.Services;
using HF.Application.Attendances.Abstract;
using HF.Application.Attendances.Services;
using HF.Application.Auth;
using HF.Application.Auth.Abstract;
using HF.Application.Auth.Validators;
using HF.Infrastructure.Auth.Requirements;
using Microsoft.AspNetCore.Authorization;
using HF.Application.ClassSessions.Abstract;
using HF.Application.ClassSessions.Services;
using HF.Application.Collaborators.Abstract;
using HF.Application.Collaborators.Services;
using HF.Application.Common.Abstractions.Repositories;
using HF.Application.Common.Middlewares;
using HF.Application.Courses.Abstract;
using HF.Application.Courses.Services;
using HF.Application.Directors.Abstract;
using HF.Application.Directors.Services;
using HF.Application.Enrollments.Abstract;
using HF.Application.Enrollments.Services;
using HF.Application.Managers.Abstract;
using HF.Application.Managers.Services;
using HF.Application.Students.Abstract;
using HF.Application.Students.Services;
using HF.Application.TeacherAssignments.Abstract;
using HF.Application.TeacherAssignments.Services;
using HF.Application.Teachers.Abstract;
using HF.Application.Teachers.Services;
using HF.Application.Volunteers.Abstract;
using HF.Application.Volunteers.Services;
using HF.Infrastructure.Auth.Models;
using HF.Infrastructure.Auth.Options;
using HF.Infrastructure.Auth.Services;
using HF.Infrastructure.Exceptions;
using HF.Infrastructure.Health.Api;
using HF.Infrastructure.Health.Database;
using HF.Infrastructure.Persistence;
using HF.Infrastructure.Persistence.Middlewares;
using HF.Infrastructure.Persistence.Repositories;
using HF.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HF.Application.Common.Utils;

namespace HF.Composition.Extensions;

public static class ServiceCollectionExtensions
{
    extension (IServiceCollection services)
    {
        public IServiceCollection AddPresentation(IHostEnvironment environment, IConfiguration? configuration = null)
        {
            _ = services.AddControllers();
            _ = services.AddOpenApi();
            _ = services.AddCors(options =>
            {
                var configuredOrigins = configuration?.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? configuration?["Cors:AllowedOrigins"]?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var origins = configuredOrigins is { Length: > 0 }
                    ? configuredOrigins
                    : [
                        "http://localhost:5076",
                        "https://localhost:7094",
                        "http://localhost:5000",
                        "https://localhost:5001",
                        "http://127.0.0.1:5076",
                        "https://127.0.0.1:7094",
                        "http://127.0.0.1:5000",
                        "https://127.0.0.1:5001"
                    ];

                options.AddPolicy("AllowWebClient", policy =>
                {
                    policy.WithOrigins(origins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });
            _ = services.AddExceptionHandler<GlobalExceptionHandler>();
            _ = services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    var httpContext = context.HttpContext;
                    var problemDetails = context.ProblemDetails;

                    var traceId = System.Diagnostics.Activity.Current?.Id ?? httpContext.TraceIdentifier;
                    problemDetails.Extensions["traceId"] = traceId;
                    problemDetails.Extensions["timestampUtc"] = DateTimeOffset.UtcNow;
                    problemDetails.Extensions["machine"] = Environment.MachineName;

                    if (environment.IsDevelopment() && context.Exception is not null)
                    {
                        problemDetails.Extensions["debug_exceptionType"] = context.Exception.GetType().FullName;
                        problemDetails.Extensions["debug_stackTrace"] = context.Exception.StackTrace;
                    }
                };
            });
            return services;
        }

        public IServiceCollection AddDatabase(IConfiguration configuration)
        {
            var rawConnectionString = configuration.GetConnectionString("PostgresConnection")
                ?? configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(rawConnectionString))
            {
                throw new InvalidOperationException(
                    "The PostgreSQL connection string is not configured. " +
                    "Set the environment variable 'ConnectionStrings__PostgresConnection' or configure 'ConnectionStrings:PostgresConnection' in appsettings.json.");
            }

            _ = services.AddDbContext<HFDbContext>(options =>
            {
                var connectionString = NormalizeNpgsqlConnectionString(rawConnectionString);
                options.UseNpgsql(connectionString, 
                builder => 
                {
                    builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                    builder.MigrationsAssembly("HF.Infrastructure");
                });
            }); 

            _ = services.AddScoped<IUnitOfWork, UnitOfWork>();
            _ = services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            _ = services.AddHostedService<DatabaseSeedHostedService>();
            return services;
        }

        public IServiceCollection AddApplication()
        {
            services.AddMediator(options => 
            {    
                options.ServiceLifetime = ServiceLifetime.Scoped;
                options.Assemblies = [typeof(Application.Auth.Handlers.AuthHandlers).Assembly];
                options.PipelineBehaviors =
                [
                    typeof(LoggingMiddleware<,>),
                    typeof(ValidationMiddleware<,>),
                    typeof(TransactionMiddleware<,>)
                ]; 
            });
            _ = services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

            _ = services.AddScoped<IStudentService, StudentService>();
            _ = services.AddScoped<ITeacherService, TeacherService>();
            _ = services.AddScoped<IManagerService, ManagerService>();
            _ = services.AddScoped<IDirectorService, DirectorService>();
            _ = services.AddScoped<ICollaboratorService, CollaboratorService>();
            _ = services.AddScoped<IVolunteerService, VolunteerService>();
            _ = services.AddScoped<ICourseService, CourseService>();
            _ = services.AddScoped<ISubjectService, SubjectService>();
            _ = services.AddScoped<IAssetService, AssetService>();
            _ = services.AddScoped<ILiabilityService, LiabilityService>();
            _ = services.AddScoped<ITeacherAssignmentService, TeacherAssignmentService>();
            _ = services.AddScoped<IEnrollmentService, EnrollmentService>();
            _ = services.AddScoped<IClassSessionService, ClassSessionService>();
            _ = services.AddScoped<IAttendanceService, AttendanceService>();

            return services;
        }

        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            _ = services.AddDatabase(configuration);
            _ = services.AddIdentity();
            _ = services.AddAuth(configuration);   
            _ = services.AddHealth();
            return services;
        }

        public IServiceCollection AddAuth(IConfiguration configuration)
        {
            // Configure options sections
            _ = services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

            // Authentication and JWT Bearer configuration
            var jwtSection = configuration.GetSection(JwtOptions.SectionName);
            var jwtKey = jwtSection["Key"];
            var jwtIssuer = jwtSection["Issuer"];
            var jwtAudience = jwtSection["Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                jwtKey = "HorizonFuture-Development-Secret-Key-Must-Be-At-Least-32-Bytes-Long-123456";
            }
            else if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
            {
                throw new InvalidOperationException(
                    "A chave JWT ('Jwt:Key') deve conter no mínimo 32 caracteres (256 bits).");
            }

            if (string.IsNullOrWhiteSpace(jwtIssuer))
            {
                jwtIssuer = "HorizonFuture";
            }

            if (string.IsNullOrWhiteSpace(jwtAudience))
            {
                jwtAudience = "HorizonFuture";
            }

            _ = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var revokedStore = context.HttpContext.RequestServices.GetRequiredService<IRevokedTokenStore>();
                        string? jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);
                        if (!string.IsNullOrEmpty(jti) && revokedStore.IsRevoked(jti))
                        {
                            context.Fail("Token has been revoked.");
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            _ = services.AddAuthorization(options =>
            {
                foreach (var permission in AppPermissions.All)
                {
                    options.AddPolicy(permission, policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(new PermissionRequirement(permission));
                    });
                }
            });
            return services;
        }

        public IServiceCollection AddHealth()
        {
            _ = services.AddHealthChecks()
                .AddCheck<ApiCheck>("ApiCheck")
                .AddCheck<HostInfoCheck>("HostInfoCheck")
                .AddCheck<DatabaseCheck>("DatabaseCheck")
                .AddCheck<DatabaseUsageCheck>("DatabaseUsageCheck");
            return services;
        }

        public IServiceCollection AddIdentity()
        {
            // Identity configuration
            services.AddIdentity<User, Role>()
                .AddRoles<Role>()
                .AddEntityFrameworkStores<HFDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedPhoneNumber = false; 
                options.SignIn.RequireConfirmedEmail = false;

                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
                
            });

            _ = services.AddScoped<IIdentityService, IdentityService>();
            _ = services.AddScoped<IIdentityAdminService, IdentityAdminService>();
            _ = services.AddScoped<IUserPermissionEvaluator, UserPermissionEvaluator>();
            _ = services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            _ = services.AddSingleton<IRevokedTokenStore, RevokedTokenStore>();
            _ = services.AddScoped<IJwtTokenService, JwtTokenService>();
            _ = services.AddScoped<IIdentityEmailSender, LoggingIdentityEmailSender>();
            return services;
        }
    }
    private static string? NormalizeNpgsqlConnectionString(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw;

        var trimmed = raw.Trim('"', '\'');
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) &&
            (uri.Scheme.Equals("postgresql", StringComparison.OrdinalIgnoreCase) ||
             uri.Scheme.Equals("postgres", StringComparison.OrdinalIgnoreCase)))
        {
            var userInfo = uri.UserInfo.Split(':', 2);
            var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');

            var isLocalOrDockerHost = uri.Host.Equals("db", StringComparison.OrdinalIgnoreCase) ||
                                      uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                                      uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);

            var builder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = port,
                Database = database,
                Username = username,
                Password = password,
                SslMode = isLocalOrDockerHost ? Npgsql.SslMode.Prefer : Npgsql.SslMode.Require
            };
            return builder.ConnectionString;
        }

        return trimmed;
    }
}