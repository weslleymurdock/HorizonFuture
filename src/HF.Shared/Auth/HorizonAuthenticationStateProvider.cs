using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using HF.Shared.Auth.Models;

namespace HF.Shared.Auth;

/// <summary>
/// Native Blazor AuthenticationStateProvider backed by ITokenStore, IUserAccessService, and JWT claims.
/// Dynamically updates authentication state across the application tree on login, refresh, logout, or session expiry.
/// </summary>
public class HorizonAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private static readonly ClaimsPrincipal AnonymousPrincipal = new(new ClaimsIdentity());

    private readonly ITokenStore _tokenStore;
    private readonly IUserAccessService? _userAccessService;
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly SemaphoreSlim _evalLock = new(1, 1);
    private bool _isDisposed;

    public HorizonAuthenticationStateProvider(
        ITokenStore tokenStore,
        IServiceProvider serviceProvider)
        : this(tokenStore, serviceProvider.GetService<IUserAccessService>(), serviceProvider)
    {
    }

    public HorizonAuthenticationStateProvider(
        ITokenStore tokenStore,
        IUserAccessService? userAccessService,
        IServiceProvider serviceProvider)
    {
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        _userAccessService = userAccessService;
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        _tokenStore.TokensChanged += OnTokensChanged;
        if (_userAccessService != null)
        {
            _userAccessService.AccessChanged += OnAccessChanged;
        }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await _evalLock.WaitAsync();
        try
        {
            var accessToken = await _tokenStore.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                if (_userAccessService != null && _userAccessService.IsLoaded)
                {
                    _userAccessService.ClearAccess();
                }
                return new AuthenticationState(AnonymousPrincipal);
            }

            // Verify if access token is expired or expiring soon
            if (JwtParser.IsExpired(accessToken, TimeSpan.FromSeconds(15)))
            {
                var refreshToken = await _tokenStore.GetRefreshTokenAsync();
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    if (_userAccessService != null && _userAccessService.IsLoaded)
                    {
                        _userAccessService.ClearAccess();
                    }
                    await _tokenStore.ClearTokensAsync();
                    return new AuthenticationState(AnonymousPrincipal);
                }

                // Attempt to refresh the session
                var refreshedPrincipal = await TryRefreshSessionAsync(refreshToken);
                if (refreshedPrincipal != null)
                {
                    return new AuthenticationState(refreshedPrincipal);
                }

                if (_userAccessService != null && _userAccessService.IsLoaded)
                {
                    _userAccessService.ClearAccess();
                }
                await _tokenStore.ClearTokensAsync();
                return new AuthenticationState(AnonymousPrincipal);
            }

            // Ensure access capabilities (roles and policies) are loaded
            if (_userAccessService != null && !_userAccessService.IsLoaded)
            {
                await _userAccessService.LoadCurrentAccessAsync();
            }

            var principal = CreatePrincipalFromJwt(accessToken);
            return new AuthenticationState(principal);
        }
        catch
        {
            // Fallback safely to unauthenticated state on unexpected storage error
            return new AuthenticationState(AnonymousPrincipal);
        }
        finally
        {
            _evalLock.Release();
        }
    }

    public void NotifyUserAuthentication(string accessToken)
    {
        var principal = CreatePrincipalFromJwt(accessToken);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public async Task<AuthenticationState> RefreshAuthenticationStateAsync()
    {
        var state = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(state));
        return state;
    }

    public void NotifyUserLogout()
    {
        _userAccessService?.ClearAccess();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(AnonymousPrincipal)));
    }

    private async void OnTokensChanged(object? sender, EventArgs e)
    {
        await NotifyCurrentStateAsync();
    }

    private async void OnAccessChanged(object? sender, EventArgs e)
    {
        await NotifyCurrentStateAsync();
    }

    private async Task NotifyCurrentStateAsync()
    {
        try
        {
            var state = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(state));
        }
        catch
        {
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(AnonymousPrincipal)));
        }
    }

    private async Task<ClaimsPrincipal?> TryRefreshSessionAsync(string refreshToken)
    {
        await _refreshLock.WaitAsync();
        try
        {
            // Re-check access token in case another concurrent operation already refreshed it
            var currentAccess = await _tokenStore.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(currentAccess) && !JwtParser.IsExpired(currentAccess, TimeSpan.FromSeconds(15)))
            {
                return CreatePrincipalFromJwt(currentAccess);
            }

            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetService<IAuthService>();
            if (authService is null)
            {
                return null;
            }

            var refreshResult = await authService.RefreshTokenAsync(new RefreshDto(refreshToken));
            if (refreshResult.Succeeded && refreshResult.Data is not null && !string.IsNullOrWhiteSpace(refreshResult.Data.AccessToken))
            {
                if (_userAccessService != null)
                {
                    await _userAccessService.LoadCurrentAccessAsync();
                }

                return CreatePrincipalFromJwt(refreshResult.Data.AccessToken);
            }

            return null;
        }
        catch
        {
            return null;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private ClaimsPrincipal CreatePrincipalFromJwt(string jwt)
    {
        var claims = JwtParser.ParseClaimsFromJwt(jwt).ToList();

        if (_userAccessService != null)
        {
            foreach (var role in _userAccessService.CurrentRoles)
            {
                if (!claims.Any(c => c.Type == ClaimTypes.Role && string.Equals(c.Value, role, StringComparison.OrdinalIgnoreCase)))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
                if (!claims.Any(c => (c.Type == "role" || c.Type == "Role") && string.Equals(c.Value, role, StringComparison.OrdinalIgnoreCase)))
                {
                    claims.Add(new Claim("role", role));
                }
            }

            foreach (var policy in _userAccessService.CurrentPolicies)
            {
                if (!claims.Any(c => (c.Type == "permission" || c.Type == "Permission") && string.Equals(c.Value, policy, StringComparison.OrdinalIgnoreCase)))
                {
                    claims.Add(new Claim("permission", policy));
                }
            }
        }

        var identity = new ClaimsIdentity(claims, "Bearer", ClaimTypes.Name, ClaimTypes.Role);
        return new ClaimsPrincipal(identity);
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _tokenStore.TokensChanged -= OnTokensChanged;
            if (_userAccessService != null)
            {
                _userAccessService.AccessChanged -= OnAccessChanged;
            }
            _refreshLock.Dispose();
            _evalLock.Dispose();
        }
    }
}
