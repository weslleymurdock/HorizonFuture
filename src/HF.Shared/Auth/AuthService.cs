using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HF.Shared.Auth.Models;

namespace HF.Shared.Auth;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStore _tokenStore;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthService(HttpClient? httpClient = null, ITokenStore? tokenStore = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _tokenStore = tokenStore ?? new InMemoryTokenStore();
    }

    private async Task SetAuthorizationHeaderAsync(HttpRequestMessage request)
    {
        var token = await _tokenStore.GetAccessTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<HttpResponseMessage> SendWithAuthAsync(HttpRequestMessage request, CancellationToken ct)
    {
        await SetAuthorizationHeaderAsync(request);
        return await _httpClient.SendAsync(request, ct);
    }

    public async Task<AuthResult<IdentityResultDto>> RegisterAsync(RegisterDto request, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/register", request, JsonOptions, ct);
            if (response.IsSuccessStatusCode)
            {
                return AuthResult<IdentityResultDto>.Ok(new IdentityResultDto(true, Array.Empty<string>()), "Usuário registrado com sucesso.");
            }

            var identityResult = await TryReadJsonAsync<IdentityResultDto>(response, ct);
            if (identityResult is not null)
            {
                return AuthResult<IdentityResultDto>.Fail(
                    string.Join("; ", identityResult.Errors),
                    identityResult.Errors,
                    (int)response.StatusCode);
            }

            var errorText = await response.Content.ReadAsStringAsync(ct);
            return AuthResult<IdentityResultDto>.Fail(
                string.IsNullOrWhiteSpace(errorText) ? "Falha ao registrar usuário." : errorText,
                statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<IdentityResultDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<TokenResponseDto>> LoginAsync(LoginDto request, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", request, JsonOptions, ct);
            if (response.IsSuccessStatusCode)
            {
                var tokenResult = await response.Content.ReadFromJsonAsync<TokenResponseDto>(JsonOptions, ct);
                if (tokenResult is not null)
                {
                    await _tokenStore.SaveTokensAsync(tokenResult.AccessToken, tokenResult.RefreshToken);
                    return AuthResult<TokenResponseDto>.Ok(tokenResult, "Login efetuado com sucesso.");
                }
            }

            var failureEnvelope = await TryReadJsonAsync<ApiResponseEnvelope<TokenResponseDto>>(response, ct);
            if (failureEnvelope is not null)
            {
                return AuthResult<TokenResponseDto>.Fail(failureEnvelope.Message, statusCode: (int)response.StatusCode);
            }

            var errorText = await response.Content.ReadAsStringAsync(ct);
            return AuthResult<TokenResponseDto>.Fail(
                string.IsNullOrWhiteSpace(errorText) ? "Credenciais inválidas ou erro no login." : errorText,
                statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<TokenResponseDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<TokenResponseDto>> RefreshTokenAsync(RefreshDto request, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/refresh", request, JsonOptions, ct);
            if (response.IsSuccessStatusCode)
            {
                var tokenResult = await response.Content.ReadFromJsonAsync<TokenResponseDto>(JsonOptions, ct);
                if (tokenResult is not null)
                {
                    await _tokenStore.SaveTokensAsync(tokenResult.AccessToken, tokenResult.RefreshToken);
                    return AuthResult<TokenResponseDto>.Ok(tokenResult, "Token renovado com sucesso.");
                }
            }

            var failureEnvelope = await TryReadJsonAsync<ApiResponseEnvelope<TokenResponseDto>>(response, ct);
            if (failureEnvelope is not null)
            {
                return AuthResult<TokenResponseDto>.Fail(failureEnvelope.Message, statusCode: (int)response.StatusCode);
            }

            return AuthResult<TokenResponseDto>.Fail("Falha ao renovar token.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<TokenResponseDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult> RevokeTokenAsync(CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/revoke");
            var response = await SendWithAuthAsync(req, ct);
            if (response.IsSuccessStatusCode)
            {
                await _tokenStore.ClearTokensAsync();
                return AuthResult.Ok("Sessão revogada com sucesso.");
            }

            var failureEnvelope = await TryReadJsonAsync<ApiResponseEnvelope<bool>>(response, ct);
            if (failureEnvelope is not null)
            {
                return AuthResult.Fail(failureEnvelope.Message, statusCode: (int)response.StatusCode);
            }

            return AuthResult.Fail("Falha ao revogar token.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<bool>> ConfirmEmailAsync(string userId, string code, string? changedEmail = null, CancellationToken ct = default)
    {
        try
        {
            var url = $"api/v1/auth/confirmEmail?userId={Uri.EscapeDataString(userId)}&code={Uri.EscapeDataString(code)}";
            if (!string.IsNullOrWhiteSpace(changedEmail))
            {
                url += $"&changedEmail={Uri.EscapeDataString(changedEmail)}";
            }

            var response = await _httpClient.GetAsync(url, ct);
            if (response.IsSuccessStatusCode)
            {
                return AuthResult<bool>.Ok(true, "Email confirmado com sucesso.");
            }

            var failureEnvelope = await TryReadJsonAsync<ApiResponseEnvelope<bool>>(response, ct);
            if (failureEnvelope is not null)
            {
                return AuthResult<bool>.Fail(failureEnvelope.Message, statusCode: (int)response.StatusCode);
            }

            return AuthResult<bool>.Fail("Falha ao confirmar email.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<bool>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<IdentityResultDto>> ResendConfirmationEmailAsync(EmailDto request, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/resendConfirmationEmail", request, JsonOptions, ct);
            var identityResult = await TryReadJsonAsync<IdentityResultDto>(response, ct);
            if (response.IsSuccessStatusCode && identityResult?.Succeeded == true)
            {
                return AuthResult<IdentityResultDto>.Ok(identityResult, "Email de confirmação reenviado.");
            }

            if (identityResult is not null)
            {
                return AuthResult<IdentityResultDto>.Fail(string.Join("; ", identityResult.Errors), identityResult.Errors, (int)response.StatusCode);
            }

            return AuthResult<IdentityResultDto>.Fail("Falha ao reenviar confirmação de email.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<IdentityResultDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<IdentityResultDto>> ForgotPasswordAsync(EmailDto request, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/forgotPassword", request, JsonOptions, ct);
            if (response.IsSuccessStatusCode)
            {
                return AuthResult<IdentityResultDto>.Ok(new IdentityResultDto(true, Array.Empty<string>()), "Instruções de recuperação enviadas.");
            }

            var identityResult = await TryReadJsonAsync<IdentityResultDto>(response, ct);
            if (identityResult is not null)
            {
                return AuthResult<IdentityResultDto>.Fail(string.Join("; ", identityResult.Errors), identityResult.Errors, (int)response.StatusCode);
            }

            return AuthResult<IdentityResultDto>.Fail("Falha ao solicitar recuperação de senha.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<IdentityResultDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<IdentityResultDto>> ResetPasswordAsync(ResetPasswordDto request, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/auth/resetPassword", request, JsonOptions, ct);
            if (response.IsSuccessStatusCode)
            {
                return AuthResult<IdentityResultDto>.Ok(new IdentityResultDto(true, Array.Empty<string>()), "Senha redefinida com sucesso.");
            }

            var identityResult = await TryReadJsonAsync<IdentityResultDto>(response, ct);
            if (identityResult is not null)
            {
                return AuthResult<IdentityResultDto>.Fail(string.Join("; ", identityResult.Errors), identityResult.Errors, (int)response.StatusCode);
            }

            return AuthResult<IdentityResultDto>.Fail("Falha ao redefinir senha.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<IdentityResultDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<IdentityInfoDto>> GetIdentityInfoAsync(CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/v1/auth/manage/info");
            var response = await SendWithAuthAsync(req, ct);
            if (response.IsSuccessStatusCode)
            {
                var info = await response.Content.ReadFromJsonAsync<IdentityInfoDto>(JsonOptions, ct);
                if (info is not null)
                {
                    return AuthResult<IdentityInfoDto>.Ok(info);
                }
            }

            return AuthResult<IdentityInfoDto>.Fail("Falha ao obter informações do usuário.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<IdentityInfoDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<IdentityResultDto>> UpdateIdentityInfoAsync(InfoDto request, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/manage/info")
            {
                Content = JsonContent.Create(request, options: JsonOptions)
            };
            var response = await SendWithAuthAsync(req, ct);
            if (response.IsSuccessStatusCode)
            {
                return AuthResult<IdentityResultDto>.Ok(new IdentityResultDto(true, Array.Empty<string>()), "Informações atualizadas.");
            }

            var identityResult = await TryReadJsonAsync<IdentityResultDto>(response, ct);
            if (identityResult is not null)
            {
                return AuthResult<IdentityResultDto>.Fail(string.Join("; ", identityResult.Errors), identityResult.Errors, (int)response.StatusCode);
            }

            return AuthResult<IdentityResultDto>.Fail("Falha ao atualizar informações.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<IdentityResultDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<TwoFactorResponseDto>> ConfigureTwoFactorAsync(TwoFactorDto request, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/manage/2fa")
            {
                Content = JsonContent.Create(request, options: JsonOptions)
            };
            var response = await SendWithAuthAsync(req, ct);
            if (response.IsSuccessStatusCode)
            {
                var twoFa = await response.Content.ReadFromJsonAsync<TwoFactorResponseDto>(JsonOptions, ct);
                if (twoFa is not null)
                {
                    return AuthResult<TwoFactorResponseDto>.Ok(twoFa, "2FA configurado com sucesso.");
                }
            }

            var identityResult = await TryReadJsonAsync<IdentityResultDto>(response, ct);
            if (identityResult is not null)
            {
                return AuthResult<TwoFactorResponseDto>.Fail(string.Join("; ", identityResult.Errors), identityResult.Errors, (int)response.StatusCode);
            }

            return AuthResult<TwoFactorResponseDto>.Fail("Falha ao configurar 2FA.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<TwoFactorResponseDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<IReadOnlyList<RoleCatalogItemDto>>> ListCatalogRolesAsync(CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/v1/auth/roles");
            var response = await SendWithAuthAsync(req, ct);
            if (response.IsSuccessStatusCode)
            {
                var envelope = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<IReadOnlyList<RoleCatalogItemDto>>>(JsonOptions, ct);
                if (envelope?.Value is not null)
                {
                    return AuthResult<IReadOnlyList<RoleCatalogItemDto>>.Ok(envelope.Value);
                }
            }

            return AuthResult<IReadOnlyList<RoleCatalogItemDto>>.Fail("Falha ao obter catálogo de roles.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<IReadOnlyList<RoleCatalogItemDto>>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<UserRolesDto>> GetUserRolesAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"api/v1/auth/users/{Uri.EscapeDataString(userId)}/roles");
            var response = await SendWithAuthAsync(req, ct);
            if (response.IsSuccessStatusCode)
            {
                var envelope = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<UserRolesDto>>(JsonOptions, ct);
                if (envelope?.Value is not null)
                {
                    return AuthResult<UserRolesDto>.Ok(envelope.Value);
                }
            }

            return AuthResult<UserRolesDto>.Fail("Falha ao obter roles do usuário.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<UserRolesDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<bool>> AssignUserRoleAsync(string userId, AssignRoleDto request, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"api/v1/auth/users/{Uri.EscapeDataString(userId)}/roles")
            {
                Content = JsonContent.Create(request, options: JsonOptions)
            };
            var response = await SendWithAuthAsync(req, ct);
            var envelope = await TryReadJsonAsync<ApiResponseEnvelope<bool>>(response, ct);
            if (response.IsSuccessStatusCode && envelope?.Success == true)
            {
                return AuthResult<bool>.Ok(true, envelope.Message);
            }

            return AuthResult<bool>.Fail(envelope?.Message ?? "Falha ao atribuir role.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<bool>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<bool>> RemoveUserRoleAsync(string userId, string roleName, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/auth/users/{Uri.EscapeDataString(userId)}/roles/{Uri.EscapeDataString(roleName)}");
            var response = await SendWithAuthAsync(req, ct);
            var envelope = await TryReadJsonAsync<ApiResponseEnvelope<bool>>(response, ct);
            if (response.IsSuccessStatusCode && envelope?.Success == true)
            {
                return AuthResult<bool>.Ok(true, envelope.Message);
            }

            return AuthResult<bool>.Fail(envelope?.Message ?? "Falha ao remover role.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<bool>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<PermissionCatalogDto>> ListCatalogPermissionsAsync(CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/v1/auth/permissions");
            var response = await SendWithAuthAsync(req, ct);
            if (response.IsSuccessStatusCode)
            {
                var envelope = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<PermissionCatalogDto>>(JsonOptions, ct);
                if (envelope?.Value is not null)
                {
                    return AuthResult<PermissionCatalogDto>.Ok(envelope.Value);
                }
            }

            return AuthResult<PermissionCatalogDto>.Fail("Falha ao obter catálogo de permissões.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<PermissionCatalogDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<UserPermissionsDto>> GetUserPermissionsAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"api/v1/auth/users/{Uri.EscapeDataString(userId)}/permissions");
            var response = await SendWithAuthAsync(req, ct);
            if (response.IsSuccessStatusCode)
            {
                var envelope = await response.Content.ReadFromJsonAsync<ApiResponseEnvelope<UserPermissionsDto>>(JsonOptions, ct);
                if (envelope?.Value is not null)
                {
                    return AuthResult<UserPermissionsDto>.Ok(envelope.Value);
                }
            }

            return AuthResult<UserPermissionsDto>.Fail("Falha ao obter permissões do usuário.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<UserPermissionsDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<bool>> GrantUserPermissionAsync(string userId, GrantPermissionDto request, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"api/v1/auth/users/{Uri.EscapeDataString(userId)}/permissions/grant")
            {
                Content = JsonContent.Create(request, options: JsonOptions)
            };
            var response = await SendWithAuthAsync(req, ct);
            var envelope = await TryReadJsonAsync<ApiResponseEnvelope<bool>>(response, ct);
            if (response.IsSuccessStatusCode && envelope?.Success == true)
            {
                return AuthResult<bool>.Ok(true, envelope.Message);
            }

            return AuthResult<bool>.Fail(envelope?.Message ?? "Falha ao conceder permissão.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<bool>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<bool>> RevokeUserPermissionAsync(string userId, RevokePermissionDto request, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"api/v1/auth/users/{Uri.EscapeDataString(userId)}/permissions/revoke")
            {
                Content = JsonContent.Create(request, options: JsonOptions)
            };
            var response = await SendWithAuthAsync(req, ct);
            var envelope = await TryReadJsonAsync<ApiResponseEnvelope<bool>>(response, ct);
            if (response.IsSuccessStatusCode && envelope?.Success == true)
            {
                return AuthResult<bool>.Ok(true, envelope.Message);
            }

            return AuthResult<bool>.Fail(envelope?.Message ?? "Falha ao revogar permissão.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<bool>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<bool>> ClearUserPermissionAsync(string userId, string permissionName, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/auth/users/{Uri.EscapeDataString(userId)}/permissions/{Uri.EscapeDataString(permissionName)}");
            var response = await SendWithAuthAsync(req, ct);
            var envelope = await TryReadJsonAsync<ApiResponseEnvelope<bool>>(response, ct);
            if (response.IsSuccessStatusCode && envelope?.Success == true)
            {
                return AuthResult<bool>.Ok(true, envelope.Message);
            }

            return AuthResult<bool>.Fail(envelope?.Message ?? "Falha ao limpar override de permissão.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<bool>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<bool>> AssignUserPermissionGroupAsync(string userId, AssignPermissionGroupDto request, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post, $"api/v1/auth/users/{Uri.EscapeDataString(userId)}/groups")
            {
                Content = JsonContent.Create(request, options: JsonOptions)
            };
            var response = await SendWithAuthAsync(req, ct);
            var envelope = await TryReadJsonAsync<ApiResponseEnvelope<bool>>(response, ct);
            if (response.IsSuccessStatusCode && envelope?.Success == true)
            {
                return AuthResult<bool>.Ok(true, envelope.Message);
            }

            return AuthResult<bool>.Fail(envelope?.Message ?? "Falha ao associar grupo.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<bool>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<bool>> RemoveUserPermissionGroupAsync(string userId, string groupName, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/auth/users/{Uri.EscapeDataString(userId)}/groups/{Uri.EscapeDataString(groupName)}");
            var response = await SendWithAuthAsync(req, ct);
            var envelope = await TryReadJsonAsync<ApiResponseEnvelope<bool>>(response, ct);
            if (response.IsSuccessStatusCode && envelope?.Success == true)
            {
                return AuthResult<bool>.Ok(true, envelope.Message);
            }

            return AuthResult<bool>.Fail(envelope?.Message ?? "Falha ao desassociar grupo.", statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<bool>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    private static async Task<T?> TryReadJsonAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
        }
        catch
        {
            return default;
        }
    }
}

