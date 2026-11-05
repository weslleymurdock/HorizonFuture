using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using HF.Client;
using HF.Shared.Auth;
using HF.Shared.PageModels;
using HF.Shared.PageModels.Auth;

namespace HF.Shared;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPageModels()
        {
            services.AddScoped<PageModelBase>();
            services.AddScoped<HomePageModel>();
            services.AddScoped<NotFoundPageModel>();

            // Kiota Client & Auth Services
            services.AddHorizonClient();

            // Auth PageModels
            services.AddScoped<LoginPageModel>();
            services.AddScoped<RegisterPageModel>();
            services.AddScoped<ForgotPasswordPageModel>();
            services.AddScoped<ResetPasswordPageModel>();
            services.AddScoped<ConfirmEmailPageModel>();
            services.AddScoped<ManageProfilePageModel>();
            services.AddScoped<ManageRolesPageModel>();
            services.AddScoped<ManagePermissionsPageModel>();

            return services;
        }

        public IServiceCollection AddHorizonClient(string? customBaseUrl = null)
        {
            services.TryAddScoped<ITokenStore, InMemoryTokenStore>();
            services.TryAddScoped<IAuthenticationProvider, KiotaAuthenticationProvider>();

            services.TryAddScoped<IRequestAdapter>(sp =>
            {
                var authProvider = sp.GetRequiredService<IAuthenticationProvider>();
                var httpClient = sp.GetService<HttpClient>() ?? new HttpClient();
                var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);

                var configuration = sp.GetService<IConfiguration>();
                var baseUrl = customBaseUrl
                    ?? configuration?["Api:BaseUrl"]
                    ?? configuration?["ApiBaseUrl"]
                    ?? httpClient.BaseAddress?.ToString()
                    ?? "http://127.0.0.1:5268";

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

            return services;
        }
    }
}
