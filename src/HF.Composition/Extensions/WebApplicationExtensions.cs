using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace HF.Composition.Extensions;

public static class WebApplicationExtensions
{
    extension (WebApplication app)
    {
        public WebApplication UseHF<T>() where T : class
        {
            app.UseExceptionHandler();
            if (!app.Environment.IsProduction())
            {
                _ = app.MapOpenApi("openapi/{v1}.json");
                _ = app.MapScalarApiReference("scalar/server", options =>
                {
                    options.WithTitle($"{app.Environment.ApplicationName} API at {app.Environment.EnvironmentName} v1")
                        .WithTheme(ScalarTheme.Moon);
                });
            }
                        
            _ = app.UseHttpsRedirection();
            _ = app.UseCors("AllowWebClient");
            _ = app.UseAuthentication();
            _ = app.UseAuthorization();
            _ = app.MapControllers();
            _ = app.MapHealthChecks("/api/v1/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    IEnumerable<object> checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                        durationMs = entry.Value.Duration.TotalMilliseconds
                    });

                    var checksResponse = new HealthCheckResponse<IEnumerable<object>>
                    (
                        report.Status.ToString(),
                        report.TotalDuration.TotalMilliseconds,
                        typeof(TaskCanceledException).Assembly.GetName().Version?.ToString() ?? "unknown",
                        app.Environment.EnvironmentName,
                        checks
                    );

                    var response = new HealthCheckResponse(
                        report.Status.ToString(),
                        report.TotalDuration.TotalMilliseconds,
                        typeof(T).Assembly.GetName().Version?.ToString() ?? "unknown",
                        app.Environment.EnvironmentName
                    );

                    if (checks.Any())
                    {
                        await context.Response.WriteAsJsonAsync(checksResponse);
                    }
                    else
                    {
                        await context.Response.WriteAsJsonAsync(response);
                    }
                }
            });

            return app;
        }
    }
}

internal class HealthCheckResponse(string status, double totalDurationMs, string version, string environment)
{
    public string Status { get; } = status;
    public double TotalDurationMs { get; } = totalDurationMs;
    public string Version { get; } = version;
    public string Environment { get; } = environment;
}

internal class HealthCheckResponse<T>(string status, double totalDurationMs, string version, string environment, T Checks)
: HealthCheckResponse(status, totalDurationMs, version, environment)
where T : IEnumerable<object>
{
    public T Checks { get; } = Checks;
}
