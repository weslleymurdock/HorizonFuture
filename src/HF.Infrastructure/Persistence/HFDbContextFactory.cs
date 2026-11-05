using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using DotEnv = dotenv.net.DotEnv;
namespace HF.Infrastructure.Persistence;

internal sealed class HFDbContextFactory : IDesignTimeDbContextFactory<HFDbContext>
{
    public HFDbContext CreateDbContext(string[] args)
    {
        DotEnv.Load();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("PostgresConnection");

        var optionsBuilder = new DbContextOptionsBuilder<HFDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new HFDbContext(optionsBuilder.Options);
    }
}