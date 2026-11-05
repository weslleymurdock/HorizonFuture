using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace HF.Shared.Auth;

/// <summary>
/// Dynamic authorization policy provider that automatically synthesizes authorization policies
/// matching backend permission names without requiring manual pre-registration.
/// Evaluates whether the user holds the requested permission claim or the Admin role.
/// </summary>
public class HorizonAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public HorizonAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var existingPolicy = await base.GetPolicyAsync(policyName);
        if (existingPolicy != null)
        {
            return existingPolicy;
        }

        // Dynamically synthesize policy matching permission name
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireAssertion(ctx =>
                ctx.User.HasClaim(c => (c.Type == "permission" || c.Type == "Permission") &&
                                       string.Equals(c.Value, policyName, StringComparison.OrdinalIgnoreCase)) ||
                ctx.User.IsInRole("Admin"))
            .Build();
    }
}

