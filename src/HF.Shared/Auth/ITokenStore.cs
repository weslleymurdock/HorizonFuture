namespace HF.Shared.Auth;

/// <summary>Abstração para gerenciamento de tokens de sessão no cliente.</summary>
public interface ITokenStore
{
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task SaveTokensAsync(string accessToken, string? refreshToken);
    Task ClearTokensAsync();
}

/// <summary>Armazenamento em memória seguro e sem persistência direta de credenciais em disco/logs.</summary>
public class InMemoryTokenStore : ITokenStore
{
    private string? _accessToken;
    private string? _refreshToken;

    public Task<string?> GetAccessTokenAsync() => Task.FromResult(_accessToken);
    public Task<string?> GetRefreshTokenAsync() => Task.FromResult(_refreshToken);

    public Task SaveTokensAsync(string accessToken, string? refreshToken)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        return Task.CompletedTask;
    }

    public Task ClearTokensAsync()
    {
        _accessToken = null;
        _refreshToken = null;
        return Task.CompletedTask;
    }
}

