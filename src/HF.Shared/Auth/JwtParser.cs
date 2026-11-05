using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;

namespace HF.Shared.Auth;

/// <summary>
/// Lightweight helper to decode JWT payload claims and expiration without heavy external dependencies.
/// </summary>
public static class JwtParser
{
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        if (string.IsNullOrWhiteSpace(jwt))
        {
            return claims;
        }

        var payload = ExtractPayload(jwt);
        if (payload is null)
        {
            return claims;
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                var claimName = MapClaimType(prop.Name);
                if (prop.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in prop.Value.EnumerateArray())
                    {
                        var val = item.GetString();
                        if (!string.IsNullOrEmpty(val))
                        {
                            claims.Add(new Claim(claimName, val));
                        }
                    }
                }
                else
                {
                    var val = prop.Value.ToString();
                    if (!string.IsNullOrEmpty(val))
                    {
                        claims.Add(new Claim(claimName, val));
                    }
                }
            }
        }
        catch
        {
            // Invalid or corrupted JWT payload
        }

        return claims;
    }

    public static DateTime? GetExpiration(string jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
        {
            return null;
        }

        var payload = ExtractPayload(jwt);
        if (payload is null)
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("exp", out var expElement) && expElement.TryGetInt64(out var expSeconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
            }
        }
        catch
        {
            // Ignored
        }

        return null;
    }

    public static bool IsExpired(string jwt, TimeSpan? clockSkew = null)
    {
        var exp = GetExpiration(jwt);
        if (!exp.HasValue)
        {
            return true;
        }

        var skew = clockSkew ?? TimeSpan.FromSeconds(30);
        return DateTime.UtcNow >= (exp.Value - skew);
    }

    private static byte[]? ExtractPayload(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2)
        {
            return null;
        }

        var payloadStr = parts[1];
        // Convert Base64Url to Base64 standard padding
        payloadStr = payloadStr.Replace('-', '+').Replace('_', '/');
        switch (payloadStr.Length % 4)
        {
            case 2: payloadStr += "=="; break;
            case 3: payloadStr += "="; break;
        }

        try
        {
            return Convert.FromBase64String(payloadStr);
        }
        catch
        {
            return null;
        }
    }

    private static string MapClaimType(string claimName) => claimName switch
    {
        "sub" => ClaimTypes.NameIdentifier,
        "nameid" => ClaimTypes.NameIdentifier,
        "email" => ClaimTypes.Email,
        "role" => ClaimTypes.Role,
        "roles" => ClaimTypes.Role,
        "name" => ClaimTypes.Name,
        _ => claimName
    };
}

