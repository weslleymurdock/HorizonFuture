using HF.Application.Auth;
using HF.Application.Auth.Abstract;
using HF.Infrastructure.Auth.Models;
using Microsoft.AspNetCore.Identity;

namespace HF.Infrastructure.Auth.Services;

/// <summary>
/// Evaluates user permissions on-demand directly against current database state.
/// Ensures that revoked roles or permissions take effect immediately with zero latency.
/// </summary>
public sealed class UserPermissionEvaluator(UserManager<User> userManager) : IUserPermissionEvaluator
{
    public async Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);

        var directGrants = claims
            .Where(c => c.Type == AppClaimTypes.Permission)
            .Select(c => c.Value);

        var explicitRevocations = claims
            .Where(c => c.Type == AppClaimTypes.RevokedPermission)
            .Select(c => c.Value);

        var assignedGroups = claims
            .Where(c => c.Type == AppClaimTypes.PermissionGroup)
            .Select(c => c.Value);

        return RolePermissionCatalog.Evaluate(roles, directGrants, explicitRevocations, assignedGroups, permission);
    }
}

