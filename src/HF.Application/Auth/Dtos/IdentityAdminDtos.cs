namespace HF.Application.Auth.Dtos;

/// <summary>Payload to assign a role to a user.</summary>
public sealed record AssignRoleDto(string RoleName);

/// <summary>Payload to grant a permission directly to a user.</summary>
public sealed record GrantPermissionDto(string PermissionName);

/// <summary>Payload to explicitly revoke a permission from a user.</summary>
public sealed record RevokePermissionDto(string PermissionName);

/// <summary>Payload to assign a user to a permission group.</summary>
public sealed record AssignPermissionGroupDto(string GroupName);

