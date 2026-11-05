using System;
using HF.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HF.Infrastructure.Health.Database;

public sealed class DatabaseUsageCheck(HFDbContext db) : IHealthCheck
{
    internal sealed record DatabaseDiskUsageResult(long Used);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await db.Database
                .SqlQueryRaw<DatabaseDiskUsageResult>(
                    """
                    SELECT pg_database_size(current_database()) AS "Used"
                    """)
                .ToListAsync(cancellationToken);

            var usage = result.SingleOrDefault()
                ?? throw new Exception("Cannot calculate database usage on disk");

            var data = new Dictionary<string, object>
            {
                ["DatabaseSize"] = new
                {
                    UsedSizeMBytes = usage.Used / 1024 / 1024
                }
            };

            return HealthCheckResult.Healthy("Database size calculated", data);
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy("Cannot calculate database usage on disk", e);
        }
    }
}