using HF.Application.Auth.Responses;
using HF.Application.Common.Responses;
using Mediator;

namespace HF.Application.Auth.Requests;

/// <summary>Query to list all roles from the catalog.</summary>
public sealed record ListCatalogRolesRequest
    : IRequest<ResponseBase<IReadOnlyList<RoleCatalogResponse>>>;

/// <summary>Query to list all permissions and groups from the catalog.</summary>
public sealed record ListCatalogPermissionsRequest
    : IRequest<ResponseBase<PermissionCatalogResponse>>;

/// <summary>Query to get assigned roles for a target user.</summary>
public sealed record GetUserRolesRequest(string TargetUserId)
    : IRequest<ResponseBase<UserRolesResponse>>;

/// <summary>Command to assign a role to a target user.</summary>
public sealed record AssignUserRoleRequest(
    string ActorUserId,
    string TargetUserId,
    string RoleName)
    : IRequest<ResponseBase<bool>>;

/// <summary>Command to remove a role from a target user.</summary>
public sealed record RemoveUserRoleRequest(
    string ActorUserId,
    string TargetUserId,
    string RoleName)
    : IRequest<ResponseBase<bool>>;

/// <summary>Query to get permissions and groups for a target user.</summary>
public sealed record GetUserPermissionsRequest(string TargetUserId)
    : IRequest<ResponseBase<UserPermissionsResponse>>;

/// <summary>Command to grant a permission directly to a target user.</summary>
public sealed record GrantUserPermissionRequest(
    string ActorUserId,
    string TargetUserId,
    string PermissionName)
    : IRequest<ResponseBase<bool>>;

/// <summary>Command to explicitly revoke a permission from a target user.</summary>
public sealed record RevokeUserPermissionRequest(
    string ActorUserId,
    string TargetUserId,
    string PermissionName)
    : IRequest<ResponseBase<bool>>;

/// <summary>Command to clear a permission override from a target user.</summary>
public sealed record ClearUserPermissionRequest(
    string ActorUserId,
    string TargetUserId,
    string PermissionName)
    : IRequest<ResponseBase<bool>>;

/// <summary>Command to assign a user to a permission group.</summary>
public sealed record AssignUserPermissionGroupRequest(
    string ActorUserId,
    string TargetUserId,
    string GroupName)
    : IRequest<ResponseBase<bool>>;

/// <summary>Command to remove a user from a permission group.</summary>
public sealed record RemoveUserPermissionGroupRequest(
    string ActorUserId,
    string TargetUserId,
    string GroupName)
    : IRequest<ResponseBase<bool>>;

/// <summary>Query to get the current authenticated user's assigned roles and effective policies.</summary>
public sealed record GetCurrentUserAccessRequest(string UserId)
    : IRequest<ResponseBase<UserAccessResponse>>;

