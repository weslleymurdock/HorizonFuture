using System;
using HF.Infrastructure.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace HF.Infrastructure.Persistence.Seed;

public class DatabaseSeedHostedService(
    IServiceProvider serviceProvider,
    ILogger<DatabaseSeedHostedService> logger) : IHostedService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<DatabaseSeedHostedService> _logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting the  Database Seed Service run...");

        using var scope = _serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        
        var dbContext = scope.ServiceProvider.GetRequiredService<HFDbContext>();
        if (dbContext.Database.GetPendingMigrations().Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        await SeedRolesAsync(roleManager);
        await SeedDefaultUserAsync(userManager);

        _logger.LogInformation("Database Seed successfully finished.");
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
                        roleName, string.Join(", ", result.Errors));
                }
            }
        }
    }

    private async Task SeedDefaultUserAsync(UserManager<User> userManager)
    {
        string adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") is { Length: > 0 } email 
            ? email 
            : throw new InvalidOperationException("A variável de ambiente 'ADMIN_EMAIL' não foi configurada.");

        string adminUserName = Environment.GetEnvironmentVariable("ADMIN_USERNAME") is { Length: > 0 } username 
            ? username 
            : throw new InvalidOperationException("A variável de ambiente 'ADMIN_USERNAME' não foi configurada.");

        string adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") is { Length: > 0 } password 
            ? password 
            : throw new InvalidOperationException("A variável de ambiente 'ADMIN_PASSWORD' não foi configurada.");
        
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

                // Atribui a role ao usuário
                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Erro ao associar role Admin ao usuário: {Errors}", 
                        string.Join(", ", roleResult.Errors));
                }
            }
            else
            {
                _logger.LogError("Erro ao criar usuário padrão: {Errors}", 
                    string.Join(", ", createResult.Errors));
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}