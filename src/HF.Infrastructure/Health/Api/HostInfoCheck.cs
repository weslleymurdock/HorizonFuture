using System;
using HF.Application.Common.Utils;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HF.Infrastructure.Health.Api;


public sealed class HostInfoCheck() : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
		try
		{
            var check = new Dictionary<string, object>
            {
                ["EnvironmentInfo"] = 
                new
                {
                    EnvironmentName = HFUtils.GetEnvironmentName(),
                    UpTime = HFUtils.GetUpTime(),
                    OSVersion = Environment.OSVersion.ToString(),
                    HostName = Environment.MachineName
                }
            };
            return Task.FromResult(HealthCheckResult.Healthy("Host is up and running", check));
        }
		catch (Exception e)
		{
			return Task.FromResult(HealthCheckResult.Unhealthy("Cannot retrieve host data", e, new Dictionary<string, object>(){  ["EnvironmentInfo"] = new { 
                EnvironmentName = HFUtils.GetEnvironmentName(), 
                UpTime = HFUtils.GetUpTime(),
                ExceptionMessage = e.Message 
            }}));
		}
    }
}