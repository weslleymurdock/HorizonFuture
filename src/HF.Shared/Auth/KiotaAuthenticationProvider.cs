using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace HF.Shared.Auth;

/// <summary>
/// Provedor de autenticação Kiota que injeta o token Bearer recuperado do ITokenStore.
/// </summary>
public class KiotaAuthenticationProvider : IAuthenticationProvider
{
    private readonly ITokenStore _tokenStore;

    public KiotaAuthenticationProvider(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = default,
        CancellationToken cancellationToken = default)
    {
        var token = await _tokenStore.GetAccessTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.TryAdd("Authorization", $"Bearer {token}");
        }
    }
}

