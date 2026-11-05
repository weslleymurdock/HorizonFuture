using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using HF.Shared.Auth;

namespace HF.Mobile.Auth;

/// <summary>
/// Cross-platform secure token store for .NET MAUI Blazor Hybrid.
/// Uses native platform keystores (Android KeyStore, iOS Keychain, Windows Credential Manager) via SecureStorage.
/// </summary>
public class MauiSecureTokenStore : ITokenStore
{
    private const string AccessTokenKey = "hf_maui_sec_at";
    private const string RefreshTokenKey = "hf_maui_sec_rt";

    private string? _fallbackAccessToken;
    private string? _fallbackRefreshToken;

    public event EventHandler? TokensChanged;

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(AccessTokenKey) ?? _fallbackAccessToken;
        }
        catch
        {
            return _fallbackAccessToken;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(RefreshTokenKey) ?? _fallbackRefreshToken;
        }
        catch
        {
            return _fallbackRefreshToken;
        }
    }

    public async Task SaveTokensAsync(string accessToken, string? refreshToken)
    {
        _fallbackAccessToken = accessToken;
        _fallbackRefreshToken = refreshToken;

        try
        {
            await SecureStorage.Default.SetAsync(AccessTokenKey, accessToken);
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
            }
            else
            {
                SecureStorage.Default.Remove(RefreshTokenKey);
            }
        }
        catch
        {
            // Fallback kept in memory
        }

        TokensChanged?.Invoke(this, EventArgs.Empty);
    }

    public Task ClearTokensAsync()
    {
        _fallbackAccessToken = null;
        _fallbackRefreshToken = null;

        try
        {
            SecureStorage.Default.Remove(AccessTokenKey);
            SecureStorage.Default.Remove(RefreshTokenKey);
        }
        catch
        {
            // Ignored
        }

        TokensChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}

