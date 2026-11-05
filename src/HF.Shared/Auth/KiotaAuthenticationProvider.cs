using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace HF.Shared.Auth;

/// <summary>
/// Kiota authentication provider that injects the Bearer access token from ITokenStore.
/// Proactively renews expiring access tokens before dispatching requests, avoiding unnecessary 401s.
/// </summary>
public class KiotaAuthenticationProvider : IAuthenticationProvider
{
    private readonly ITokenStore _tokenStore;
    private readonly Func<string>? _baseUrlProvider;
    private static readonly SemaphoreSlim ProactiveRefreshLock = new(1, 1);

    public KiotaAuthenticationProvider(ITokenStore tokenStore, Func<string>? baseUrlProvider = null)
    {
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        _baseUrlProvider = baseUrlProvider;
    }

    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = default,
        CancellationToken cancellationToken = default)
    {
        var uri = request.URI?.ToString() ?? string.Empty;
        if (IsAuthBypassUri(uri))
        {
            return;
        }

        var token = await _tokenStore.GetAccessTokenAsync();

        // Proactive token refresh if expiring soon
        if (!string.IsNullOrWhiteSpace(token) && JwtParser.IsExpired(token, TimeSpan.FromSeconds(30)) && _baseUrlProvider != null)
        {
            token = await RefreshProactivelyAsync(token, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Remove("Authorization");
            request.Headers.TryAdd("Authorization", $"Bearer {token}");
        }
    }

    private async Task<string?> RefreshProactivelyAsync(string expiringToken, CancellationToken cancellationToken)
    {
        await ProactiveRefreshLock.WaitAsync(cancellationToken);
        try
        {
            // Re-check in case another request already rotated the token
            var currentToken = await _tokenStore.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(currentToken) && !JwtParser.IsExpired(currentToken, TimeSpan.FromSeconds(30)))
            {
                return currentToken;
            }

            var refreshToken = await _tokenStore.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return currentToken;
            }

            var baseUrl = _baseUrlProvider!().TrimEnd('/');
            using var client = new HttpClient();
            var payload = JsonSerializer.Serialize(new { refreshToken });
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{baseUrl}/api/v1/auth/refresh", content, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
                var root = doc.RootElement;
                var target = root.TryGetProperty("value", out var val) && val.ValueKind == JsonValueKind.Object ? val : root;

                if (target.TryGetProperty("accessToken", out var atElem))
                {
                    var newAccess = atElem.GetString();
                    var newRefresh = target.TryGetProperty("refreshToken", out var rtElem) ? rtElem.GetString() : null;

                    if (!string.IsNullOrWhiteSpace(newAccess))
                    {
                        await _tokenStore.SaveTokensAsync(newAccess, newRefresh ?? refreshToken);
                        return newAccess;
                    }
                }
            }
        }
        catch
        {
            // Fail safely: return existing token or let request proceed to delegating handler
        }
        finally
        {
            ProactiveRefreshLock.Release();
        }

        return await _tokenStore.GetAccessTokenAsync();
    }

    private static bool IsAuthBypassUri(string uri)
    {
        return uri.Contains("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase) ||
               uri.Contains("/api/v1/auth/refresh", StringComparison.OrdinalIgnoreCase) ||
               uri.Contains("/api/v1/auth/register", StringComparison.OrdinalIgnoreCase) ||
               uri.Contains("/api/v1/auth/forgotPassword", StringComparison.OrdinalIgnoreCase) ||
               uri.Contains("/api/v1/auth/resetPassword", StringComparison.OrdinalIgnoreCase) ||
               uri.Contains("/api/v1/auth/confirmEmail", StringComparison.OrdinalIgnoreCase) ||
               uri.Contains("/api/v1/auth/resendConfirmationEmail", StringComparison.OrdinalIgnoreCase);
    }
}
