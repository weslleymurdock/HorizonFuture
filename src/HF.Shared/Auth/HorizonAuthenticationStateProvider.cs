using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using HF.Shared.Auth.Models;

namespace HF.Shared.Auth;

/// <summary>
/// Native Blazor AuthenticationStateProvider backed by ITokenStore and JWT claims.
/// Dynamically updates authentication state across the application tree on login, refresh, logout, or session expiry.
/// </summary>
public class HorizonAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private static readonly ClaimsPrincipal AnonymousPrincipal = new(new ClaimsIdentity());

    private readonly ITokenStore _tokenStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private bool _isDisposed;

    public HorizonAuthenticationStateProvider(ITokenStore tokenStore, IServiceProvider serviceProvider)
    {
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        _tokenStore.TokensChanged += OnTokensChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var accessToken = await _tokenStore.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return new AuthenticationState(AnonymousPrincipal);
            }

            // Verify if access token is expired or expiring soon
            if (JwtParser.IsExpired(accessToken, TimeSpan.FromSeconds(15)))
            {
                var refreshToken = await _tokenStore.GetRefreshTokenAsync();
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    await _tokenStore.ClearTokensAsync();
                    return new AuthenticationState(AnonymousPrincipal);
                }

                // Attempt to refresh the session
                var refreshedPrincipal = await TryRefreshSessionAsync(refreshToken);
                if (refreshedPrincipal != null)
                {
                    return new AuthenticationState(refreshedPrincipal);
                }

                await _tokenStore.ClearTokensAsync();
                return new AuthenticationState(AnonymousPrincipal);
            }

            var principal = CreatePrincipalFromJwt(accessToken);
            return new AuthenticationState(principal);
        }
        catch
        {
            // Fallback safely to unauthenticated state on unexpected storage error
            return new AuthenticationState(AnonymousPrincipal);
        }
    }

    public void NotifyUserAuthentication(string accessToken)
    {
        var principal = CreatePrincipalFromJwt(accessToken);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(AnonymousPrincipal)));
    }

    private void OnTokensChanged(object? sender, EventArgs e)
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
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

    private static ClaimsPrincipal CreatePrincipalFromJwt(string jwt)
    {
        var claims = JwtParser.ParseClaimsFromJwt(jwt);
        var identity = new ClaimsIdentity(claims, "Bearer");
        return new ClaimsPrincipal(identity);
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _tokenStore.TokensChanged -= OnTokensChanged;
            _refreshLock.Dispose();
        }
    }
}

