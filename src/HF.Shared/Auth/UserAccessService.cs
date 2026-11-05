using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using HF.Shared.Auth.Models;

namespace HF.Shared.Auth;

/// <summary>
/// Thread-safe in-memory store for the active user's roles and effective policies.
/// Fetches capabilities from GET /api/v1/auth/access using IAuthService without cyclic dependencies.
/// </summary>
public class UserAccessService : IUserAccessService
{
    private readonly IAuthService _authService;
    private readonly ITokenStore _tokenStore;
    private readonly SemaphoreSlim _asyncLock = new(1, 1);
    private readonly object _stateLock = new();

    private List<string> _currentRoles = new();
    private List<string> _currentPolicies = new();
    private bool _isLoaded;

    public event EventHandler? AccessChanged;

    public UserAccessService(IAuthService authService, ITokenStore tokenStore)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
    }

    public IReadOnlyList<string> CurrentRoles => _currentRoles;
    public IReadOnlyList<string> CurrentPolicies => _currentPolicies;
    public bool IsLoaded => _isLoaded;

    public bool HasRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        return _currentRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasPolicy(string policy)
    {
        if (string.IsNullOrWhiteSpace(policy))
        {
            return false;
        }

        return _currentRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase) ||
               _currentPolicies.Contains(policy, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<bool> LoadCurrentAccessAsync(CancellationToken ct = default)
    {
        await _asyncLock.WaitAsync(ct);
        bool notifyChanged = false;
        try
        {
            var token = await _tokenStore.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                lock (_stateLock)
                {
                    notifyChanged = ClearInternal();
                }
                return false;
            }

            var result = await _authService.GetCurrentUserAccessAsync(ct);
            if (result.Succeeded && result.Data is not null)
            {
                var newRoles = result.Data.Roles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                var newPolicies = result.Data.Policies.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                lock (_stateLock)
                {
                    notifyChanged = !_isLoaded ||
                                    !_currentRoles.SequenceEqual(newRoles, StringComparer.OrdinalIgnoreCase) ||
                                    !_currentPolicies.SequenceEqual(newPolicies, StringComparer.OrdinalIgnoreCase);

                    _currentRoles = newRoles;
                    _currentPolicies = newPolicies;
                    _isLoaded = true;
                }
                return true;
            }

            if (result.StatusCode == 401)
            {
                lock (_stateLock)
                {
                    notifyChanged = ClearInternal();
                }
                await _tokenStore.ClearTokensAsync();
                return false;
            }

            // Transient network failure: keep existing capabilities or fallback to JWT
            if (!_isLoaded && !string.IsNullOrWhiteSpace(token))
            {
                var jwtClaims = JwtParser.ParseClaimsFromJwt(token);
                var jwtRoles = jwtClaims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (jwtRoles.Count > 0)
                {
                    lock (_stateLock)
                    {
                        notifyChanged = !_isLoaded || !_currentRoles.SequenceEqual(jwtRoles, StringComparer.OrdinalIgnoreCase);
                        _currentRoles = jwtRoles;
                        _isLoaded = true;
                    }
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
        finally
        {
            _asyncLock.Release();
            if (notifyChanged)
            {
                AccessChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void SetAccess(IEnumerable<string> roles, IEnumerable<string> policies)
    {
        bool notifyChanged = false;
        lock (_stateLock)
        {
            var newRoles = roles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var newPolicies = policies.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            notifyChanged = !_isLoaded ||
                            !_currentRoles.SequenceEqual(newRoles, StringComparer.OrdinalIgnoreCase) ||
                            !_currentPolicies.SequenceEqual(newPolicies, StringComparer.OrdinalIgnoreCase);

            _currentRoles = newRoles;
            _currentPolicies = newPolicies;
            _isLoaded = true;
        }

        if (notifyChanged)
        {
            AccessChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ClearAccess()
    {
        bool notifyChanged = false;
        lock (_stateLock)
        {
            notifyChanged = ClearInternal();
        }

        if (notifyChanged)
        {
            AccessChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool ClearInternal()
    {
        bool hadAccess = _isLoaded || _currentRoles.Count > 0 || _currentPolicies.Count > 0;
        _currentRoles = new List<string>();
        _currentPolicies = new List<string>();
        _isLoaded = false;
        return hadAccess;
    }
}

