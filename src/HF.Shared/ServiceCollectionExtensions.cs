using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using HF.Client;
using HF.Shared.Auth;
using HF.Shared.Courses;
using HF.Shared.Subjects;
using HF.Shared.Students;
using HF.Shared.Teachers;
using HF.Shared.PageModels;
using HF.Shared.PageModels.Auth;
using HF.Shared.PageModels.Courses;
using HF.Shared.PageModels.Subjects;
using HF.Shared.PageModels.Students;
using HF.Shared.PageModels.Teachers;

namespace HF.Shared;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPageModels(string? customBaseUrl = null)
        {
            services.AddScoped<PageModelBase>();
            services.AddScoped<HomePageModel>();
            services.AddScoped<NotFoundPageModel>();

            // Kiota Client & Auth Services
            services.AddHorizonClient(customBaseUrl);

            // Auth PageModels
            services.AddScoped<LoginPageModel>();
            services.AddScoped<RegisterPageModel>();
            services.AddScoped<ForgotPasswordPageModel>();
            services.AddScoped<ResetPasswordPageModel>();
            services.AddScoped<ConfirmEmailPageModel>();
            services.AddScoped<ManageProfilePageModel>();
            services.AddScoped<ManageRolesPageModel>();
            services.AddScoped<ManagePermissionsPageModel>();

            // Courses PageModels
            services.AddScoped<CourseListPageModel>();
            services.AddScoped<CourseCreatePageModel>();
            services.AddScoped<CourseDetailsPageModel>();
            services.AddScoped<CourseEditPageModel>();

            // Subjects PageModels
            services.AddScoped<SubjectListPageModel>();
            services.AddScoped<SubjectCreatePageModel>();
            services.AddScoped<SubjectDetailsPageModel>();
            services.AddScoped<SubjectEditPageModel>();

            // Students PageModels
            services.AddScoped<StudentListPageModel>();
            services.AddScoped<StudentCreatePageModel>();
            services.AddScoped<StudentDetailsPageModel>();
            services.AddScoped<StudentEditPageModel>();

            // Teachers PageModels
            services.AddScoped<TeacherListPageModel>();
            services.AddScoped<TeacherCreatePageModel>();
            services.AddScoped<TeacherDetailsPageModel>();
            services.AddScoped<TeacherEditPageModel>();

            return services;
        }

        public IServiceCollection AddHorizonClient(string? customBaseUrl = null)
        {
            services.AddAuthorizationCore();
            services.TryAddScoped<ITokenStore, InMemoryTokenStore>();
            services.TryAddScoped<AuthenticationStateProvider, HorizonAuthenticationStateProvider>();
            services.TryAddScoped<HorizonAuthenticationStateProvider>(sp => (HorizonAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

            services.TryAddScoped<RefreshTokenHandler>(sp =>
            {
                var tokenStore = sp.GetRequiredService<ITokenStore>();
                var configuration = sp.GetService<IConfiguration>();
                var baseUrl = customBaseUrl
                    ?? configuration?["Api:BaseUrl"]
                    ?? configuration?["ApiBaseUrl"]
                    ?? Environment.GetEnvironmentVariable("HF_API_BASE_URL")
                    ?? "http://localhost:8080";

                return new RefreshTokenHandler(tokenStore, () => baseUrl)
                {
                    InnerHandler = new HttpClientHandler()
                };
            });

            services.TryAddScoped<IAuthenticationProvider>(sp =>
            {
                var tokenStore = sp.GetRequiredService<ITokenStore>();
                var configuration = sp.GetService<IConfiguration>();
                var baseUrl = customBaseUrl
                    ?? configuration?["Api:BaseUrl"]
                    ?? configuration?["ApiBaseUrl"]
                    ?? Environment.GetEnvironmentVariable("HF_API_BASE_URL")
                    ?? "http://localhost:8080";

                return new KiotaAuthenticationProvider(tokenStore, () => baseUrl);
            });

            services.TryAddScoped<IRequestAdapter>(sp =>
            {
                var authProvider = sp.GetRequiredService<IAuthenticationProvider>();
                var refreshHandler = sp.GetService<RefreshTokenHandler>();
                var httpClient = refreshHandler != null ? new HttpClient(refreshHandler) : (sp.GetService<HttpClient>() ?? new HttpClient());
                var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);

                var configuration = sp.GetService<IConfiguration>();
                var baseUrl = customBaseUrl
                    ?? configuration?["Api:BaseUrl"]
                    ?? configuration?["ApiBaseUrl"]
                    ?? Environment.GetEnvironmentVariable("HF_API_BASE_URL")
                    ?? "http://localhost:8080";

                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    adapter.BaseUrl = baseUrl.TrimEnd('/');
                }

                return adapter;
            });

            services.TryAddScoped<ApiClient>(sp =>
            {
                var adapter = sp.GetRequiredService<IRequestAdapter>();
                return new ApiClient(adapter);
            });

            services.TryAddScoped<IAuthService, AuthService>();
            services.TryAddScoped<ICourseService, CourseService>();
            services.TryAddScoped<ISubjectService, SubjectService>();
            services.TryAddScoped<IStudentService, StudentService>();
            services.TryAddScoped<ITeacherService, TeacherService>();

            return services;
        }
    }
}
