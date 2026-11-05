namespace HF.Application.Auth;

/// <summary>
/// Custom claim types used for per-user permissions, revocations, and groups.
/// </summary>
public static class AppClaimTypes
{
    /// <summary>Directly granted permission capability.</summary>
    public const string Permission = "permission";

    /// <summary>Explicitly revoked permission capability (takes precedence over role and group inheritance).</summary>
    public const string RevokedPermission = "revoked_permission";

    /// <summary>Assigned named permission group.</summary>
    public const string PermissionGroup = "permission_group";
}

