using System;
using Microsoft.Extensions.DependencyInjection;
using HF.Shared.PageModels;
using HF.Shared.PageModels.Auth;
using HF.Shared.Auth;

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

            // Auth Shared Services
            services.AddScoped<ITokenStore, InMemoryTokenStore>();
            services.AddScoped<IAuthService, AuthService>();

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
    }
}
