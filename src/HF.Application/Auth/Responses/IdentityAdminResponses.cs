namespace HF.Application.Auth.Responses;

/// <summary>Catalog role entry with hierarchy rank.</summary>
public sealed record RoleCatalogResponse(string RoleName, int Rank);

/// <summary>Catalog permissions and available permission groups.</summary>
public sealed record PermissionCatalogResponse(
    IReadOnlyList<string> Permissions,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Groups);

/// <summary>Assigned roles for a user.</summary>
public sealed record UserRolesResponse(
    string UserId,
    IReadOnlyList<string> Roles);

/// <summary>Effective permissions, direct grants, explicit revocations, and groups for a user.</summary>
public sealed record UserPermissionsResponse(
    string UserId,
    IReadOnlyList<string> EffectivePermissions,
    IReadOnlyList<string> DirectGrants,
    IReadOnlyList<string> ExplicitRevocations,
    IReadOnlyList<string> AssignedGroups);

