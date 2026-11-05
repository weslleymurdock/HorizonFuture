using System;
using Microsoft.Extensions.DependencyInjection;
using HF.Shared.PageModels;

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
            return services;
        }
    }
}
