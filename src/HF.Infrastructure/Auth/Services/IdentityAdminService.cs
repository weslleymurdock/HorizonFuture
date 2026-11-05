using System.Security.Claims;
using HF.Application.Auth;
using HF.Application.Auth.Abstract;
using HF.Application.Auth.Responses;
using HF.Application.Common.Responses;
using HF.Infrastructure.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace HF.Infrastructure.Auth.Services;

/// <summary>
/// Implements secure identity administration for roles, permissions, and permission groups.
/// Enforces server-side authority validation, anti-self-elevation, and precedence rules.
/// </summary>
public sealed class IdentityAdminService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    ILogger<IdentityAdminService> logger)
    : IIdentityAdminService
{
    /// <inheritdoc />
    public Task<ResponseBase<IReadOnlyList<RoleCatalogResponse>>> ListCatalogRolesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var roles = AppRoles.All
            .Select(name => new RoleCatalogResponse(name, AppRoles.GetRank(name)))
            .ToList();

        return Task.FromResult(ResponseBase<IReadOnlyList<RoleCatalogResponse>>.SuccessResponse(roles));
    }

    /// <inheritdoc />
    public Task<ResponseBase<PermissionCatalogResponse>> ListCatalogPermissionsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var response = new PermissionCatalogResponse(
            AppPermissions.All,
            AppPermissionGroups.GroupDefinitions);

        return Task.FromResult(ResponseBase<PermissionCatalogResponse>.SuccessResponse(response));
    }

    /// <inheritdoc />
    public async Task<ResponseBase<UserRolesResponse>> GetUserRolesAsync(string targetUserId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var target = await userManager.FindByIdAsync(targetUserId);
        if (target is null)
        {
            return ResponseBase<UserRolesResponse>.FailureResponse("Target user not found.");
        }

        var roles = await userManager.GetRolesAsync(target);
        return ResponseBase<UserRolesResponse>.SuccessResponse(new UserRolesResponse(targetUserId, roles.ToList()));
    }

    /// <inheritdoc />
    public async Task<ResponseBase<bool>> AssignRoleAsync(
        string actorUserId,
        string targetUserId,
        string roleName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (actorUserId == targetUserId)
        {
            return ResponseBase<bool>.FailureResponse("Actors cannot alter their own roles.");
        }

        if (!AppRoles.IsValidRole(roleName))
        {
            return ResponseBase<bool>.FailureResponse($"Role '{roleName}' does not exist in authorized catalog.");
        }

        if (string.Equals(roleName, AppRoles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            return ResponseBase<bool>.FailureResponse("The Admin role cannot be granted through regular role management. It must be provisioned through system bootstrap.");
        }

        var actor = await userManager.FindByIdAsync(actorUserId);
        if (actor is null)
        {
            return ResponseBase<bool>.FailureResponse("Actor user not found.");
        }

        var target = await userManager.FindByIdAsync(targetUserId);
        if (target is null)
        {
            return ResponseBase<bool>.FailureResponse("Target user not found.");
        }

        var actorRoles = await userManager.GetRolesAsync(actor);
        var targetRoles = await userManager.GetRolesAsync(target);

        int actorRank = AppRoles.GetUserMaxRank(actorRoles);
        int targetRank = AppRoles.GetUserMaxRank(targetRoles);
        int roleRank = AppRoles.GetRank(roleName);

        if (actorRank <= targetRank)
        {
            return ResponseBase<bool>.FailureResponse($"Actor authority (rank {actorRank}) is not strictly higher than target user authority (rank {targetRank}).");
        }

        if (actorRank <= roleRank)
        {
            return ResponseBase<bool>.FailureResponse($"Actor authority (rank {actorRank}) cannot assign role '{roleName}' of equal or higher rank ({roleRank}).");
        }

        if (await userManager.IsInRoleAsync(target, roleName))
        {
            return ResponseBase<bool>.SuccessResponse(true, $"Role '{roleName}' is already assigned to user.");
        }

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new Role(roleName));
        }

        var result = await userManager.AddToRoleAsync(target, roleName);
        if (!result.Succeeded)
        {
            return ResponseBase<bool>.FailureResponse(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        logger.LogInformation(
            "SecurityAudit: Actor {ActorUserId} assigned role {RoleName} to User {TargetUserId} at {TimestampUtc}",
            actorUserId,
            roleName,
            targetUserId,
            DateTime.UtcNow);

        return ResponseBase<bool>.SuccessResponse(true, $"Role '{roleName}' assigned successfully.");
    }

    /// <inheritdoc />
    public async Task<ResponseBase<bool>> RemoveRoleAsync(
        string actorUserId,
        string targetUserId,
        string roleName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (actorUserId == targetUserId)
        {
            return ResponseBase<bool>.FailureResponse("Actors cannot alter their own roles.");
        }

        if (!AppRoles.IsValidRole(roleName))
        {
            return ResponseBase<bool>.FailureResponse($"Role '{roleName}' does not exist in authorized catalog.");
        }

        var actor = await userManager.FindByIdAsync(actorUserId);
        if (actor is null)
        {
            return ResponseBase<bool>.FailureResponse("Actor user not found.");
        }

        var target = await userManager.FindByIdAsync(targetUserId);
        if (target is null)
        {
            return ResponseBase<bool>.FailureResponse("Target user not found.");
        }

        if (!await userManager.IsInRoleAsync(target, roleName))
        {
            return ResponseBase<bool>.FailureResponse($"Target user does not possess role '{roleName}'.");
        }

        var actorRoles = await userManager.GetRolesAsync(actor);
        var targetRoles = await userManager.GetRolesAsync(target);

        int actorRank = AppRoles.GetUserMaxRank(actorRoles);
        int targetRank = AppRoles.GetUserMaxRank(targetRoles);
        int roleRank = AppRoles.GetRank(roleName);

        if (actorRank <= targetRank)
        {
            return ResponseBase<bool>.FailureResponse($"Actor authority (rank {actorRank}) is not strictly higher than target user authority (rank {targetRank}).");
        }

        if (actorRank <= roleRank)
        {
            return ResponseBase<bool>.FailureResponse($"Actor authority (rank {actorRank}) cannot remove role '{roleName}' of equal or higher rank ({roleRank}).");
        }

        if (string.Equals(roleName, AppRoles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            var admins = await userManager.GetUsersInRoleAsync(AppRoles.Admin);
            if (admins.Count <= 1)
            {
                return ResponseBase<bool>.FailureResponse("Cannot remove the last administrator from the system.");
            }
        }

        var result = await userManager.RemoveFromRoleAsync(target, roleName);
        if (!result.Succeeded)
        {
            return ResponseBase<bool>.FailureResponse(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        logger.LogInformation(
            "SecurityAudit: Actor {ActorUserId} removed role {RoleName} from User {TargetUserId} at {TimestampUtc}",
            actorUserId,
            roleName,
            targetUserId,
            DateTime.UtcNow);

        return ResponseBase<bool>.SuccessResponse(true, $"Role '{roleName}' removed successfully.");
    }

    /// <inheritdoc />
    public async Task<ResponseBase<UserPermissionsResponse>> GetUserPermissionsAsync(string targetUserId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var target = await userManager.FindByIdAsync(targetUserId);
        if (target is null)
        {
            return ResponseBase<UserPermissionsResponse>.FailureResponse("Target user not found.");
        }

        var roles = await userManager.GetRolesAsync(target);
        var claims = await userManager.GetClaimsAsync(target);

        var directGrants = claims
            .Where(c => c.Type == AppClaimTypes.Permission)
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var explicitRevocations = claims
            .Where(c => c.Type == AppClaimTypes.RevokedPermission)
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var assignedGroups = claims
            .Where(c => c.Type == AppClaimTypes.PermissionGroup)
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var effective = RolePermissionCatalog.GetEffectivePermissions(roles, directGrants, explicitRevocations, assignedGroups);

        return ResponseBase<UserPermissionsResponse>.SuccessResponse(
            new UserPermissionsResponse(targetUserId, effective, directGrants, explicitRevocations, assignedGroups));
    }

    /// <inheritdoc />
    public async Task<ResponseBase<bool>> GrantPermissionAsync(
        string actorUserId,
        string targetUserId,
        string permissionName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (actorUserId == targetUserId)
        {
            return ResponseBase<bool>.FailureResponse("Actors cannot grant permissions to themselves.");
        }

        if (!AppPermissions.IsValidPermission(permissionName))
        {
            return ResponseBase<bool>.FailureResponse($"Permission '{permissionName}' does not exist in authorized catalog.");
        }

        var actor = await userManager.FindByIdAsync(actorUserId);
        if (actor is null)
        {
            return ResponseBase<bool>.FailureResponse("Actor user not found.");
        }

        var target = await userManager.FindByIdAsync(targetUserId);
        if (target is null)
        {
            return ResponseBase<bool>.FailureResponse("Target user not found.");
        }

        int actorRank = AppRoles.GetUserMaxRank(await userManager.GetRolesAsync(actor));
        int targetRank = AppRoles.GetUserMaxRank(await userManager.GetRolesAsync(target));
        int requiredRank = AppPermissions.GetRequiredGrantRank(permissionName);

        if (actorRank <= targetRank)
        {
            return ResponseBase<bool>.FailureResponse($"Actor authority (rank {actorRank}) is not strictly higher than target user authority (rank {targetRank}).");
        }

        if (actorRank <= requiredRank)
        {
            return ResponseBase<bool>.FailureResponse($"Actor authority (rank {actorRank}) is insufficient to grant permission '{permissionName}' (requires rank > {requiredRank}).");
        }

        var claims = await userManager.GetClaimsAsync(target);

        // Remove explicit revocation if present so grant takes immediate effect
        var existingRevocation = claims.FirstOrDefault(c => c.Type == AppClaimTypes.RevokedPermission && string.Equals(c.Value, permissionName, StringComparison.OrdinalIgnoreCase));
        if (existingRevocation is not null)
        {
            await userManager.RemoveClaimAsync(target, existingRevocation);
        }

        // Add direct grant if not already present
        var existingGrant = claims.FirstOrDefault(c => c.Type == AppClaimTypes.Permission && string.Equals(c.Value, permissionName, StringComparison.OrdinalIgnoreCase));
        if (existingGrant is null)
        {
            var result = await userManager.AddClaimAsync(target, new Claim(AppClaimTypes.Permission, permissionName));
            if (!result.Succeeded)
            {
                return ResponseBase<bool>.FailureResponse(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        logger.LogInformation(
            "SecurityAudit: Actor {ActorUserId} granted permission {PermissionName} to User {TargetUserId} at {TimestampUtc}",
            actorUserId,
            permissionName,
            targetUserId,
            DateTime.UtcNow);

        return ResponseBase<bool>.SuccessResponse(true, $"Permission '{permissionName}' granted successfully.");
    }

    /// <inheritdoc />
    public async Task<ResponseBase<bool>> RevokePermissionAsync(
        string actorUserId,
        string targetUserId,
        string permissionName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (actorUserId == targetUserId)
        {
            return ResponseBase<bool>.FailureResponse("Actors cannot revoke permissions from themselves.");
        }

        if (!AppPermissions.IsValidPermission(permissionName))
        {
            return ResponseBase<bool>.FailureResponse($"Permission '{permissionName}' does not exist in authorized catalog.");
        }

        var actor = await userManager.FindByIdAsync(actorUserId);
        if (actor is null)
        {
            return ResponseBase<bool>.FailureResponse("Actor user not found.");
        }

        var target = await userManager.FindByIdAsync(targetUserId);
        if (target is null)
        {
            return ResponseBase<bool>.FailureResponse("Target user not found.");
        }

        int actorRank = AppRoles.GetUserMaxRank(await userManager.GetRolesAsync(actor));
        int targetRank = AppRoles.GetUserMaxRank(await userManager.GetRolesAsync(target));
        int requiredRank = AppPermissions.GetRequiredGrantRank(permissionName);

        if (actorRank <= targetRank)
        {
            return ResponseBase<bool>.FailureResponse($"Actor authority (rank {actorRank}) is not strictly higher than target user authority (rank {targetRank}).");
        }

        if (actorRank <= requiredRank)
        {
            return ResponseBase<bool>.FailureResponse($"Actor authority (rank {actorRank}) is insufficient to revoke permission '{permissionName}' (requires rank > {requiredRank}).");
        }

        var claims = await userManager.GetClaimsAsync(target);

        // Remove direct grant if present
        var existingGrant = claims.FirstOrDefault(c => c.Type == AppClaimTypes.Permission && string.Equals(c.Value, permissionName, StringComparison.OrdinalIgnoreCase));
        if (existingGrant is not null)
        {
            await userManager.RemoveClaimAsync(target, existingGrant);
        }

        // Add explicit revocation if not already present
        var existingRevocation = claims.FirstOrDefault(c => c.Type == AppClaimTypes.RevokedPermission && string.Equals(c.Value, permissionName, StringComparison.OrdinalIgnoreCase));
        if (existingRevocation is null)
        {
            var result = await userManager.AddClaimAsync(target, new Claim(AppClaimTypes.RevokedPermission, permissionName));
            if (!result.Succeeded)
            {
                return ResponseBase<bool>.FailureResponse(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        logger.LogInformation(
            "SecurityAudit: Actor {ActorUserId} revoked permission {PermissionName} from User {TargetUserId} at {TimestampUtc}",
            actorUserId,
            permissionName,
            targetUserId,
            DateTime.UtcNow);

        return ResponseBase<bool>.SuccessResponse(true, $"Permission '{permissionName}' revoked successfully.");
    }

    /// <inheritdoc />
    public async Task<ResponseBase<bool>> ClearPermissionOverrideAsync(
        string actorUserId,
        string targetUserId,
        string permissionName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (actorUserId == targetUserId)
        {
            return ResponseBase<bool>.FailureResponse("Actors cannot clear permission overrides on themselves.");
        }

        if (!AppPermissions.IsValidPermission(permissionName))
        {
            return ResponseBase<bool>.FailureResponse($"Permission '{permissionName}' does not exist in authorized catalog.");
        }

        var actor = await userManager.FindByIdAsync(actorUserId);
        if (actor is null)
        {
            return ResponseBase<bool>.FailureResponse("Actor user not found.");
        }

        var target = await userManager.FindByIdAsync(targetUserId);
        if (target is null)
        {
            return ResponseBase<bool>.FailureResponse("Target user not found.");
        }

        int actorRank = AppRoles.GetUserMaxRank(await userManager.GetRolesAsync(actor));
        int targetRank = AppRoles.GetUserMaxRank(await userManager.GetRolesAsync(target));
        int requiredRank = AppPermissions.GetRequiredGrantRank(permissionName);

        if (actorRank <= targetRank)
        {
            return ResponseBase<bool>.FailureResponse($"Actor authority (rank {actorRank}) is not strictly higher than target user authority (rank {targetRank}).");
        }

        if (actorRank <= requiredRank)
        {
            return ResponseBase<bool>.FailureResponse($"Actor authority (rank {actorRank}) is insufficient to clear override for '{permissionName}' (requires rank > {requiredRank}).");
        }

        var claims = await userManager.GetClaimsAsync(target);
        var grant = claims.FirstOrDefault(c => c.Type == AppClaimTypes.Permission && string.Equals(c.Value, permissionName, StringComparison.OrdinalIgnoreCase));
        if (grant is not null)
        {
            await userManager.RemoveClaimAsync(target, grant);
        }

        var revocation = claims.FirstOrDefault(c => c.Type == AppClaimTypes.RevokedPermission && string.Equals(c.Value, permissionName, StringComparison.OrdinalIgnoreCase));
        if (revocation is not null)
        {
            await userManager.RemoveClaimAsync(target, revocation);
        }

        logger.LogInformation(
            "SecurityAudit: Actor {ActorUserId} cleared permission override for {PermissionName} on User {TargetUserId} at {TimestampUtc}",
            actorUserId,
            permissionName,
            targetUserId,
            DateTime.UtcNow);

        return ResponseBase<bool>.SuccessResponse(true, $"Permission override for '{permissionName}' cleared successfully.");
    }

    /// <inheritdoc />
    public async Task<ResponseBase<bool>> AssignPermissionGroupAsync(
        string actorUserId,
        string targetUserId,
        string groupName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (actorUserId == targetUserId)
        {
            return ResponseBase<bool>.FailureResponse("Actors cannot assign permission groups to themselves.");
        }

        if (!AppPermissionGroups.IsValidGroup(groupName))
        {
            return ResponseBase<bool>.FailureResponse($"Permission group '{groupName}' does not exist in authorized catalog.");
        }

        var actor = await userManager.FindByIdAsync(actorUserId);
        if (actor is null)
        {
            return ResponseBase<bool>.FailureResponse("Actor user not found.");
        }

        var target = await userManager.FindByIdAsync(targetUserId);
        if (target is null)
        {
            return ResponseBase<bool>.FailureResponse("Target user not found.");
        }

        int actorRank = AppRoles.GetUserMaxRank(await userManager.GetRolesAsync(actor));
        int targetRank = AppRoles.GetUserMaxRank(await userManager.GetRolesAsync(target));
        int requiredRank = AppPermissionGroups.GetRequiredGrantRank(groupName);

        if (actorRank <= targetRank)
        {
            return ResponseBase<bool>.FailureResponse($"Actor authority (rank {actorRank}) is not strictly higher than target user authority (rank {targetRank}).");
        }

        if (actorRank <= requiredRank)
        {
            return ResponseBase<bool>.FailureResponse($"Actor authority (rank {actorRank}) is insufficient to assign group '{groupName}' (requires rank > {requiredRank}).");
        }

        var claims = await userManager.GetClaimsAsync(target);
        var existing = claims.FirstOrDefault(c => c.Type == AppClaimTypes.PermissionGroup && string.Equals(c.Value, groupName, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            var result = await userManager.AddClaimAsync(target, new Claim(AppClaimTypes.PermissionGroup, groupName));
            if (!result.Succeeded)
            {
                return ResponseBase<bool>.FailureResponse(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        logger.LogInformation(
            "SecurityAudit: Actor {ActorUserId} assigned permission group {GroupName} to User {TargetUserId} at {TimestampUtc}",
            actorUserId,
            groupName,
            targetUserId,
            DateTime.UtcNow);

        return ResponseBase<bool>.SuccessResponse(true, $"Permission group '{groupName}' assigned successfully.");
    }

    /// <inheritdoc />
    public async Task<ResponseBase<bool>> RemovePermissionGroupAsync(
        string actorUserId,
        string targetUserId,
        string groupName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (actorUserId == targetUserId)
        {
            return ResponseBase<bool>.FailureResponse("Actors cannot remove permission groups from themselves.");
        }

        if (!AppPermissionGroups.IsValidGroup(groupName))
        {
            return ResponseBase<bool>.FailureResponse($"Permission group '{groupName}' does not exist in authorized catalog.");
        }

        var actor = await userManager.FindByIdAsync(actorUserId);
        if (actor is null)
        {
            return ResponseBase<bool>.FailureResponse("Actor user not found.");
        }

        var target = await userManager.FindByIdAsync(targetUserId);
        if (target is null)
        {
            return ResponseBase<bool>.FailureResponse("Target user not found.");
        }

        int actorRank = AppRoles.GetUserMaxRank(await userManager.GetRolesAsync(actor));
        int targetRank = AppRoles.GetUserMaxRank(await userManager.GetRolesAsync(target));
        int requiredRank = AppPermissionGroups.GetRequiredGrantRank(groupName);

        if (actorRank <= targetRank)
        {
            return ResponseBase<bool>.FailureResponse($"Actor authority (rank {actorRank}) is not strictly higher than target user authority (rank {targetRank}).");
        }

        if (actorRank <= requiredRank)
        {
            return ResponseBase<bool>.FailureResponse($"Actor authority (rank {actorRank}) is insufficient to remove group '{groupName}' (requires rank > {requiredRank}).");
        }

        var claims = await userManager.GetClaimsAsync(target);
        var existing = claims.FirstOrDefault(c => c.Type == AppClaimTypes.PermissionGroup && string.Equals(c.Value, groupName, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            var result = await userManager.RemoveClaimAsync(target, existing);
            if (!result.Succeeded)
            {
                return ResponseBase<bool>.FailureResponse(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        logger.LogInformation(
            "SecurityAudit: Actor {ActorUserId} removed permission group {GroupName} from User {TargetUserId} at {TimestampUtc}",
            actorUserId,
            groupName,
            targetUserId,
            DateTime.UtcNow);

        return ResponseBase<bool>.SuccessResponse(true, $"Permission group '{groupName}' removed successfully.");
    }

    /// <inheritdoc />
    public async Task<ResponseBase<UserAccessResponse>> GetUserAccessAsync(string targetUserId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var target = await userManager.FindByIdAsync(targetUserId);
        if (target is null)
        {
            return ResponseBase<UserAccessResponse>.FailureResponse("User not found.");
        }

        var roles = await userManager.GetRolesAsync(target);
        var claims = await userManager.GetClaimsAsync(target);

        var directGrants = claims
            .Where(c => c.Type == AppClaimTypes.Permission)
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var explicitRevocations = claims
            .Where(c => c.Type == AppClaimTypes.RevokedPermission)
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var assignedGroups = claims
            .Where(c => c.Type == AppClaimTypes.PermissionGroup)
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var effective = RolePermissionCatalog.GetEffectivePermissions(roles, directGrants, explicitRevocations, assignedGroups);

        return ResponseBase<UserAccessResponse>.SuccessResponse(
            new UserAccessResponse(roles.ToList(), effective));
    }
}

