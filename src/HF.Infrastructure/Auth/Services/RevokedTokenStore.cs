using System;
using System.Collections.Concurrent;
using HF.Application.Auth.Abstract;

namespace HF.Infrastructure.Auth.Services;


/// <summary>Stores revoked token identifiers in process until their natural expiration.</summary>
public sealed class RevokedTokenStore : IRevokedTokenStore
{
    private readonly ConcurrentDictionary<string, DateTime> _tokens = new();

    /// <inheritdoc />
    public bool IsRevoked(string tokenId)
    {
        if (!_tokens.TryGetValue(tokenId, out DateTime expiresAt))
        {
            return false;
        }

        if (expiresAt > DateTime.UtcNow)
        {
            return true;
        }

        _ = _tokens.TryRemove(tokenId, out _);
        return false;
    }

    /// <inheritdoc />
    public void Revoke(string tokenId, DateTime expiresAt)
    {
        if (expiresAt > DateTime.UtcNow)
        {
            _tokens[tokenId] = expiresAt;
        }
    }
}