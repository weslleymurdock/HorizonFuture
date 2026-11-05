using System.Security.Claims;
using HF.Infrastructure.Auth.Models;
using Microsoft.AspNetCore.Identity;

namespace HF.Application.Tests.Common;

public class InMemoryUserStore : IUserStore<User>, IUserRoleStore<User>, IUserClaimStore<User>
{
    private readonly Dictionary<string, User> _users = [];
    private readonly Dictionary<string, HashSet<string>> _userRoles = [];
    private readonly Dictionary<string, List<Claim>> _userClaims = [];

    public void AddUser(User user)
    {
        _users[user.Id] = user;
        if (!_userRoles.ContainsKey(user.Id))
        {
            _userRoles[user.Id] = [];
        }

        if (!_userClaims.ContainsKey(user.Id))
        {
            _userClaims[user.Id] = [];
        }
    }

    public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.Id);

    public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);

    public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);

    public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
    {
        AddUser(user);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
    {
        _users[user.Id] = user;
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
    {
        _users.Remove(user.Id);
        _userRoles.Remove(user.Id);
        _userClaims.Remove(user.Id);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        _users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        var user = _users.Values.FirstOrDefault(u => string.Equals(u.NormalizedUserName, normalizedUserName, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
    {
        if (!_userRoles.TryGetValue(user.Id, out var roles))
        {
            roles = [];
            _userRoles[user.Id] = roles;
        }

        roles.Add(roleName);
        return Task.CompletedTask;
    }

    public Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
    {
        if (_userRoles.TryGetValue(user.Id, out var roles))
        {
            roles.Remove(roleName);
        }

        return Task.CompletedTask;
    }

    public Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
    {
        if (_userRoles.TryGetValue(user.Id, out var roles))
        {
            return Task.FromResult<IList<string>>(roles.ToList());
        }

        return Task.FromResult<IList<string>>([]);
    }

    public Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
    {
        bool inRole = _userRoles.TryGetValue(user.Id, out var roles) && roles.Contains(roleName);
        return Task.FromResult(inRole);
    }

    public Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        var users = _users.Values
            .Where(u => _userRoles.TryGetValue(u.Id, out var r) && r.Contains(roleName))
            .ToList();

        return Task.FromResult<IList<User>>(users);
    }

    public Task<IList<Claim>> GetClaimsAsync(User user, CancellationToken cancellationToken)
    {
        if (_userClaims.TryGetValue(user.Id, out var claims))
        {
            return Task.FromResult<IList<Claim>>(claims.ToList());
        }

        return Task.FromResult<IList<Claim>>([]);
    }

    public Task AddClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        if (!_userClaims.TryGetValue(user.Id, out var userClaims))
        {
            userClaims = [];
            _userClaims[user.Id] = userClaims;
        }

        userClaims.AddRange(claims);
        return Task.CompletedTask;
    }

    public Task ReplaceClaimAsync(User user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
    {
        if (_userClaims.TryGetValue(user.Id, out var claims))
        {
            claims.Remove(claim);
            claims.Add(newClaim);
        }

        return Task.CompletedTask;
    }

    public Task RemoveClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        if (_userClaims.TryGetValue(user.Id, out var userClaims))
        {
            foreach (var claim in claims)
            {
                userClaims.RemoveAll(c => c.Type == claim.Type && c.Value == claim.Value);
            }
        }

        return Task.CompletedTask;
    }

    public Task<IList<User>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
    {
        var users = _users.Values
            .Where(u => _userClaims.TryGetValue(u.Id, out var cList) && cList.Any(c => c.Type == claim.Type && c.Value == claim.Value))
            .ToList();

        return Task.FromResult<IList<User>>(users);
    }

    public void Dispose()
    {
    }
}

public class InMemoryRoleStore : IRoleStore<Role>
{
    private readonly Dictionary<string, Role> _roles = [];

    public Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
    {
        _roles[role.Id] = role;
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
    {
        _roles[role.Id] = role;
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
    {
        _roles.Remove(role.Id);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken) => Task.FromResult(role.Id);

    public Task<string?> GetRoleNameAsync(Role role, CancellationToken cancellationToken) => Task.FromResult(role.Name);

    public Task SetRoleNameAsync(Role role, string? roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken) => Task.FromResult(role.NormalizedName);

    public Task SetNormalizedRoleNameAsync(Role role, string? normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public Task<Role?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        _roles.TryGetValue(roleId, out var role);
        return Task.FromResult(role);
    }

    public Task<Role?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        var role = _roles.Values.FirstOrDefault(r => string.Equals(r.Name, normalizedRoleName, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(role);
    }

    public void Dispose()
    {
    }
}

