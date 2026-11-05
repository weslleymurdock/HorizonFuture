using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HF.Shared.Auth.Models;

namespace HF.Shared.Auth;

/// <summary>
/// Delegating HTTP handler that intercepts 401 Unauthorized responses to perform reactive token refresh and replay the request.
/// Prevents concurrent refresh stampedes, loops, deadlocks, and recursive refresh calls.
/// </summary>
public class RefreshTokenHandler : DelegatingHandler
{
    private readonly ITokenStore _tokenStore;
    private readonly Func<string> _baseUrlProvider;
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    public RefreshTokenHandler(ITokenStore tokenStore, Func<string> baseUrlProvider)
    {
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        _baseUrlProvider = baseUrlProvider ?? throw new ArgumentNullException(nameof(baseUrlProvider));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Don't intercept auth endpoints to avoid recursive refresh loops
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        if (IsAuthBypassPath(path))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        // Buffer the request content in case we need to clone and replay it on 401
        byte[]? originalContentBytes = null;
        MediaTypeHeaderValue? originalContentType = null;
        if (request.Content != null)
        {
            originalContentBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            originalContentType = request.Content.Headers.ContentType;
            request.Content = new ByteArrayContent(originalContentBytes);
            if (originalContentType != null)
            {
                request.Content.Headers.ContentType = originalContentType;
            }
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        // Check if token can be refreshed
        var originalAuthHeader = request.Headers.Authorization?.Parameter;
        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            var currentAccessToken = await _tokenStore.GetAccessTokenAsync();

            // 1. If another concurrent request already rotated the token, replay with the new token
            if (!string.IsNullOrWhiteSpace(currentAccessToken) && currentAccessToken != originalAuthHeader)
            {
                response.Dispose();
                var retryRequest = CloneRequest(request, originalContentBytes, originalContentType, currentAccessToken);
                return await base.SendAsync(retryRequest, cancellationToken);
            }

            // 2. Otherwise, attempt refresh
            var refreshToken = await _tokenStore.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                await _tokenStore.ClearTokensAsync();
                return response;
            }

            var newTokens = await ExecuteRefreshAsync(refreshToken, cancellationToken);
            if (newTokens != null && !string.IsNullOrWhiteSpace(newTokens.AccessToken))
            {
                await _tokenStore.SaveTokensAsync(newTokens.AccessToken, newTokens.RefreshToken);

                // Replay the original request with the new rotated token
                response.Dispose();
                var retryRequest = CloneRequest(request, originalContentBytes, originalContentType, newTokens.AccessToken);
                return await base.SendAsync(retryRequest, cancellationToken);
            }

            // Refresh failed (invalid/revoked refresh token)
            await _tokenStore.ClearTokensAsync();
            return response;
        }
        catch (HttpRequestException)
        {
            // Network failure: preserve session tokens so intermittent connectivity doesn't log the user out
            return response;
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private async Task<TokenResponseDto?> ExecuteRefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var baseUrl = _baseUrlProvider().TrimEnd('/');
        var refreshEndpoint = $"{baseUrl}/api/v1/auth/refresh";

        using var refreshRequest = new HttpRequestMessage(HttpMethod.Post, refreshEndpoint);
        var jsonPayload = JsonSerializer.Serialize(new { refreshToken });
        refreshRequest.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        // Use InnerHandler directly so this request bypasses this delegating handler
        var invoker = InnerHandler != null ? new HttpMessageInvoker(InnerHandler) : null;
        var refreshResponse = invoker != null
            ? await invoker.SendAsync(refreshRequest, cancellationToken)
            : await base.SendAsync(refreshRequest, cancellationToken);

        if (!refreshResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var responseStream = await refreshResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        var root = doc.RootElement;

        // The API returns either TokenResponse directly or enveloped in ResponseBase
        JsonElement tokenElement = root;
        if (root.TryGetProperty("value", out var val) && val.ValueKind == JsonValueKind.Object)
        {
            tokenElement = val;
        }

        if (tokenElement.TryGetProperty("accessToken", out var atElem))
        {
            var accessToken = atElem.GetString() ?? string.Empty;
            var rt = tokenElement.TryGetProperty("refreshToken", out var rtElem) ? rtElem.GetString() : string.Empty;
            var tokenType = tokenElement.TryGetProperty("tokenType", out var ttElem) ? ttElem.GetString() : "Bearer";
            var expiresIn = tokenElement.TryGetProperty("expiresIn", out var expElem) ? expElem.GetInt32() : 3600;

            return new TokenResponseDto(tokenType ?? "Bearer", accessToken, expiresIn, rt ?? string.Empty);
        }

        return null;
    }

    private static HttpRequestMessage CloneRequest(
        HttpRequestMessage original,
        byte[]? contentBytes,
        MediaTypeHeaderValue? contentType,
        string newAccessToken)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);
        foreach (var header in original.Headers)
        {
            if (string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        clone.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);

        if (contentBytes != null)
        {
            clone.Content = new ByteArrayContent(contentBytes);
            if (contentType != null)
            {
                clone.Content.Headers.ContentType = contentType;
            }
        }

        return clone;
    }

    private static bool IsAuthBypassPath(string path)
    {
        return path.Contains("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("/api/v1/auth/refresh", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("/api/v1/auth/register", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("/api/v1/auth/forgotPassword", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("/api/v1/auth/resetPassword", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("/api/v1/auth/confirmEmail", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("/api/v1/auth/resendConfirmationEmail", StringComparison.OrdinalIgnoreCase);
    }
}

