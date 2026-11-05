using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HF.Application.Auth.Abstract;
using Microsoft.AspNetCore.Authorization;

namespace HF.Infrastructure.Auth.Requirements;

/// <summary>
/// Dynamic authorization handler that resolves permissions per request against the user's current effective capabilities.
/// </summary>
public sealed class PermissionAuthorizationHandler(IUserPermissionEvaluator permissionEvaluator)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                     ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        if (await permissionEvaluator.HasPermissionAsync(userId, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}

