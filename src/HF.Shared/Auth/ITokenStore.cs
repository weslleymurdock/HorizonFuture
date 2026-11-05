using System;
using System.Threading.Tasks;

namespace HF.Shared.Auth;

/// <summary>
/// Abstraction for client-side authentication token management.
/// </summary>
public interface ITokenStore
{
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task SaveTokensAsync(string accessToken, string? refreshToken);
    Task ClearTokensAsync();
    event EventHandler? TokensChanged;
}

/// <summary>
/// Safe in-memory token store without persistence to disk or logs.
/// Used as a safe fallback or in test fixtures.
/// </summary>
public class InMemoryTokenStore : ITokenStore
{
    private string? _accessToken;
    private string? _refreshToken;

    public event EventHandler? TokensChanged;

    public Task<string?> GetAccessTokenAsync() => Task.FromResult(_accessToken);
    public Task<string?> GetRefreshTokenAsync() => Task.FromResult(_refreshToken);

    public Task SaveTokensAsync(string accessToken, string? refreshToken)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        TokensChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task ClearTokensAsync()
    {
        _accessToken = null;
        _refreshToken = null;
        TokensChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
