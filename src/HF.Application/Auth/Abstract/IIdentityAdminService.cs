using HF.Application.Auth.Responses;
using HF.Application.Common.Responses;

namespace HF.Application.Auth.Abstract;

/// <summary>
/// Service abstraction for secure role and permission management.
/// Enforces server-side authority validation, anti-self-elevation, and precedence rules.
/// </summary>
public interface IIdentityAdminService
{
    /// <summary>Lists all roles in the catalog with their hierarchy ranks.</summary>
    Task<ResponseBase<IReadOnlyList<RoleCatalogResponse>>> ListCatalogRolesAsync(CancellationToken cancellationToken);

    /// <summary>Lists all permissions and named groups in the catalog.</summary>
    Task<ResponseBase<PermissionCatalogResponse>> ListCatalogPermissionsAsync(CancellationToken cancellationToken);

    /// <summary>Gets the assigned roles for a target user.</summary>
    Task<ResponseBase<UserRolesResponse>> GetUserRolesAsync(string targetUserId, CancellationToken cancellationToken);

    /// <summary>Assigns a role to a target user, validating actor precedence and preventing self-elevation.</summary>
    Task<ResponseBase<bool>> AssignRoleAsync(string actorUserId, string targetUserId, string roleName, CancellationToken cancellationToken);

    /// <summary>Removes a role from a target user, validating actor precedence, preventing self-elevation, and preventing removal of the last administrator.</summary>
    Task<ResponseBase<bool>> RemoveRoleAsync(string actorUserId, string targetUserId, string roleName, CancellationToken cancellationToken);

    /// <summary>Gets effective permissions, direct grants, revocations, and groups for a target user.</summary>
    Task<ResponseBase<UserPermissionsResponse>> GetUserPermissionsAsync(string targetUserId, CancellationToken cancellationToken);

    /// <summary>Grants a permission directly to a target user.</summary>
    Task<ResponseBase<bool>> GrantPermissionAsync(string actorUserId, string targetUserId, string permissionName, CancellationToken cancellationToken);

    /// <summary>Explicitly revokes a permission from a target user.</summary>
    Task<ResponseBase<bool>> RevokePermissionAsync(string actorUserId, string targetUserId, string permissionName, CancellationToken cancellationToken);

    /// <summary>Clears any direct grant or explicit revocation override for a permission on a target user.</summary>
    Task<ResponseBase<bool>> ClearPermissionOverrideAsync(string actorUserId, string targetUserId, string permissionName, CancellationToken cancellationToken);

    /// <summary>Assigns a target user to a predefined permission group.</summary>
    Task<ResponseBase<bool>> AssignPermissionGroupAsync(string actorUserId, string targetUserId, string groupName, CancellationToken cancellationToken);

    /// <summary>Removes a target user from a permission group.</summary>
    Task<ResponseBase<bool>> RemovePermissionGroupAsync(string actorUserId, string targetUserId, string groupName, CancellationToken cancellationToken);
}

