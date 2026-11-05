using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace HF.Composition.Extensions;

public static class WebApplicationBuilderExtensions
{
    extension (WebApplicationBuilder builder)
    {
        public WebApplicationBuilder BuildHF(IHostEnvironment environment)
        {
            _ = builder.Services.AddPresentation(environment);
            _ = builder.Services.AddApplication(); 
            _ = builder.Services.AddInfrastructure(builder.Configuration);
            return builder;
        }
    }
}
