using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;

#pragma warning disable CA1416 // Validate platform compatibility

namespace HF.Shared.Auth;

/// <summary>
/// Authenticated encrypted browser token store for Blazor WebAssembly.
/// Tokens are encrypted using AES-256-CBC and signed with HMAC-SHA256 (Encrypt-then-MAC)
/// before being stored in browser localStorage, preventing plaintext exposure in developer tools or passive scrapers.
/// 
/// Security Guarantees & Threat Model:
/// - Guarantees: Prevents cleartext JWT and refresh token exposure in browser storage inspectors, simple at-rest scrapers, and accidental disk dumps.
/// - Limitations: Because the WebAssembly runtime and encryption key reside on the client browser environment, this encryption DOES NOT protect against Cross-Site Scripting (XSS). If malicious script executes within the origin, it can invoke or inspect the same runtime decryption routines.
/// </summary>
public class EncryptedBrowserTokenStore : ITokenStore
{
    private const string AccessTokenKey = "hf_sec_at";
    private const string RefreshTokenKey = "hf_sec_rt";

    private static readonly byte[] EncryptionKey = DeriveKey("EncryptionKey");
    private static readonly byte[] HmacKey = DeriveKey("HmacSigningKey");

    private readonly IJSRuntime _jsRuntime;
    private string? _cachedAccessToken;
    private string? _cachedRefreshToken;
    private bool _hasLoadedFromStorage;
    public event EventHandler? TokensChanged;

    public EncryptedBrowserTokenStore(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        if (_cachedAccessToken != null)
        {
            return _cachedAccessToken;
        }

        if (!_hasLoadedFromStorage)
        {
            _cachedAccessToken = await ReadEncryptedAsync(AccessTokenKey);
            _hasLoadedFromStorage = true;
        }

        return _cachedAccessToken;
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        if (_cachedRefreshToken != null)
        {
            return _cachedRefreshToken;
        }

        _cachedRefreshToken = await ReadEncryptedAsync(RefreshTokenKey);
        return _cachedRefreshToken;
    }

    public async Task SaveTokensAsync(string accessToken, string? refreshToken)
    {
        _cachedAccessToken = accessToken;
        _cachedRefreshToken = refreshToken;
        _hasLoadedFromStorage = true;

        await WriteEncryptedAsync(AccessTokenKey, accessToken);
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await WriteEncryptedAsync(RefreshTokenKey, refreshToken);
        }
        else
        {
            await RemoveKeyAsync(RefreshTokenKey);
        }

        TokensChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task ClearTokensAsync()
    {
        _cachedAccessToken = null;
        _cachedRefreshToken = null;
        _hasLoadedFromStorage = true;

        await RemoveKeyAsync(AccessTokenKey);
        await RemoveKeyAsync(RefreshTokenKey);
        TokensChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task<string?> ReadEncryptedAsync(string key)
    {
        try
        {
            var rawEncrypted = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            if (string.IsNullOrWhiteSpace(rawEncrypted))
            {
                return null;
            }

            return Decrypt(rawEncrypted);
        }
        catch (CryptographicException)
        {
            // Data corruption or tampering detected: clear invalid data safely
            await RemoveKeyAsync(key);
            return null;
        }
        catch
        {
            // Storage unavailable or JS disconnected
            return null;
        }
    }

    private async Task WriteEncryptedAsync(string key, string plaintext)
    {
        try
        {
            var encrypted = Encrypt(plaintext);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, encrypted);
        }
        catch
        {
            // Storage quota exceeded or disabled
        }
    }

    private async Task RemoveKeyAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch
        {
            // Ignored
        }
    }

    private static string Encrypt(string plaintext)
    {
        byte[] iv = new byte[16];
        RandomNumberGenerator.Fill(iv);

        byte[] ciphertext;
        using (var aes = Aes.Create())
        {
            aes.Key = EncryptionKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                cs.Write(plaintextBytes, 0, plaintextBytes.Length);
                cs.FlushFinalBlock();
            }
            ciphertext = ms.ToArray();
        }

        // Compute HMAC-SHA256 over IV + Ciphertext
        byte[] payloadToSign = new byte[iv.Length + ciphertext.Length];
        Buffer.BlockCopy(iv, 0, payloadToSign, 0, iv.Length);
        Buffer.BlockCopy(ciphertext, 0, payloadToSign, iv.Length, ciphertext.Length);

        byte[] hmac;
        using (var hmacSha256 = new HMACSHA256(HmacKey))
        {
            hmac = hmacSha256.ComputeHash(payloadToSign);
        }

        // Pack format: IV (16) + HMAC (32) + Ciphertext
        byte[] packed = new byte[iv.Length + hmac.Length + ciphertext.Length];
        Buffer.BlockCopy(iv, 0, packed, 0, iv.Length);
        Buffer.BlockCopy(hmac, 0, packed, iv.Length, hmac.Length);
        Buffer.BlockCopy(ciphertext, 0, packed, iv.Length + hmac.Length, ciphertext.Length);

        return Convert.ToBase64String(packed);
    }

    private static string? Decrypt(string encryptedBase64)
    {
        byte[] packed = Convert.FromBase64String(encryptedBase64);
        if (packed.Length < 48) // 16 (IV) + 32 (HMAC)
        {
            return null;
        }

        byte[] iv = new byte[16];
        byte[] hmac = new byte[32];
        int cipherLength = packed.Length - 48;
        byte[] ciphertext = new byte[cipherLength];

        Buffer.BlockCopy(packed, 0, iv, 0, 16);
        Buffer.BlockCopy(packed, 16, hmac, 0, 32);
        Buffer.BlockCopy(packed, 48, ciphertext, 0, cipherLength);

        // Verify HMAC
        byte[] payloadToVerify = new byte[iv.Length + ciphertext.Length];
        Buffer.BlockCopy(iv, 0, payloadToVerify, 0, iv.Length);
        Buffer.BlockCopy(ciphertext, 0, payloadToVerify, iv.Length, ciphertext.Length);

        using (var hmacSha256 = new HMACSHA256(HmacKey))
        {
            byte[] computedHmac = hmacSha256.ComputeHash(payloadToVerify);
            if (!CryptographicOperations.FixedTimeEquals(hmac, computedHmac))
            {
                throw new CryptographicException("HMAC signature verification failed: tampered or corrupted token.");
            }
        }

        // Decrypt
        using var aes = Aes.Create();
        aes.Key = EncryptionKey;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(ciphertext);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs, Encoding.UTF8);

        return reader.ReadToEnd();
    }

    private static byte[] DeriveKey(string purpose)
    {
        var seed = Encoding.UTF8.GetBytes($"HorizonFuture:BlazorWasm:ClientStorage:v1:{purpose}");
        return SHA256.HashData(seed);
    }
}

