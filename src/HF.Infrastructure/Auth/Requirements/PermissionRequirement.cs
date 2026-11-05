using Microsoft.AspNetCore.Authorization;

namespace HF.Infrastructure.Auth.Requirements;

/// <summary>
/// Authorization requirement that validates whether a user holds a specific stable permission capability.
/// </summary>
public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

