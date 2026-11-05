using System;
using HF.Application.Common.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace HF.Composition.Extensions;

public static class WebApplicationBuilderExtensions
{
    extension (WebApplicationBuilder builder)
    {
        public WebApplicationBuilder BuildHF(IHostEnvironment environment)
        {
            _ = builder.Services.AddPresentation(environment).AddLogging();
            _ = builder.Services.AddApplication(); 
            _ = builder.Services.AddInfrastructure(builder.Configuration);
            return builder;
        }

        public IServiceCollection AddLogging()
        {
                        
            if (!Directory.Exists(Constants.Directories.Logs))
            {
                Directory.CreateDirectory(Constants.Directories.Logs);
            }

            string path = System.IO.Path.Combine(Constants.Directories.Logs, "log-.txt");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel
                .Is(Serilog.Events.LogEventLevel.Fatal)
                .WriteTo
                .Console(Serilog.Events.LogEventLevel.Verbose, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo
                .File(
                    path: path,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            builder.Logging.AddSerilog(Log.Logger, dispose: true);
            builder.Logging.AddConsole();
            #if DEBUG
            builder.Logging.AddDebug();
            #endif
            return builder.Services;
        }
    }
}
