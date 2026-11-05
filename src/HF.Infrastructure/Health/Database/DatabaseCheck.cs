using System;
using HF.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HF.Infrastructure.Health.Database;

public sealed class DatabaseCheck(HFDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await db.Database.CanConnectAsync(cancellationToken) ? 
                HealthCheckResult.Healthy("Database is reachable", 
                    new Dictionary<string, object>() { { "DatabaseInfo", new { CanConnect = true } } }) : 
                    HealthCheckResult.Unhealthy("Database is not reachable", 
                    null, 
                    new Dictionary<string, object>() { { "DatabaseInfo", new { CanConnect = false } } });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Exception while checking database", ex);
        }
    }

}