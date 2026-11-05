using System;
using System.Linq;
using HF.Infrastructure.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HF.Infrastructure.Persistence.Seed;

public class DatabaseSeedHostedService(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<DatabaseSeedHostedService> logger) : IHostedService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<DatabaseSeedHostedService> _logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting the  Database Seed Service run...");

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HFDbContext>();

        bool canConnect = false;
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            canConnect = await dbContext.Database.CanConnectAsync(timeoutCts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "It was not possible to connect to the database during initialization: {Message}", ex.Message);
        }

        if (!canConnect)
        {
            _logger.LogWarning("PostgreSQL database unavailable during initialization. Migrations and seed will be ignored. The /api/v1/health endpoint will report the database unavailability.");
            return;
        }

        var autoMigrate = Environment.GetEnvironmentVariable("DATABASE_AUTO_MIGRATE") == "true";

        var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pendingMigrations.Count > 0)
        {
            if (autoMigrate)
            {
                _logger.LogInformation("Aplicando {Count} migrations pendentes autorizadas pela configuração...", pendingMigrations.Count);
                await dbContext.Database.MigrateAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("Existem {Count} migrations pendentes, mas a execução automática de migrations não está habilitada. Seeding ignorado até aplicação deliberada das migrations.", pendingMigrations.Count);
                return;
            }
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        try
        {
            await SeedRolesAsync(roleManager);
            await SeedDefaultUserAsync(userManager);
            _logger.LogInformation("Database seed concluído com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a execução do seed no banco de dados.");
        }
    }

    private async Task SeedRolesAsync(RoleManager<Role> roleManager)
    {
        string[] roles = ["Admin", "Director", "Coordinator", "Manager", "Coworker", "Volunteer", "Reader"];

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new Role(roleName));
                if (result.Succeeded)
                {
                    _logger.LogInformation("Role criada: {Role}", roleName);
                }
                else
                {
                    _logger.LogError("Erro ao criar Role {Role}: {Errors}", 
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private async Task SeedDefaultUserAsync(UserManager<User> userManager)
    {
        var adminEmail = _configuration["ADMIN_EMAIL"] ?? _configuration["AdminUser:Email"];
        var adminUserName = _configuration["ADMIN_USERNAME"] ?? _configuration["AdminUser:UserName"];
        var adminPassword = _configuration["ADMIN_PASSWORD"] ?? _configuration["AdminUser:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminUserName) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            _logger.LogInformation("Criação de usuário administrador padrão ignorada: ADMIN_EMAIL, ADMIN_USERNAME ou ADMIN_PASSWORD não configurados.");
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(adminEmail);

        if (existingUser == null)
        {
            var adminUser = new User
            {
                UserName = adminUserName,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (createResult.Succeeded)
            {
                _logger.LogInformation("Usuário padrão criado com sucesso: {Email}", adminEmail);

                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Erro ao associar role Admin ao usuário: {Errors}", 
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                _logger.LogError("Erro ao criar usuário padrão: {Errors}", 
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
