namespace HF.Application.Common.Utils;

public static class HFUtils
{
    /// <summary>Gets the current environment name.</summary>
    public static string GetEnvironmentName()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
    }

    /// <summary>Gets the application uptime.</summary>
    public static TimeSpan GetUpTime()
    {
        return DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
    }
}
