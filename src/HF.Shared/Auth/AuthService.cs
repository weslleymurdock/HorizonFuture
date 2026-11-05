using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HF.Client;
using HF.Client.Models;
using HF.Shared.Auth.Models;
using Microsoft.Kiota.Abstractions;

namespace HF.Shared.Auth;

/// <summary>
/// Serviço compartilhado que encapsula o cliente Kiota de HF.Client,
/// adaptando as chamadas REST para os PageModels e a camada de UI sem construir requisições manuais.
/// </summary>
public class AuthService : IAuthService
{
    private readonly ApiClient _apiClient;
    private readonly ITokenStore _tokenStore;

    public AuthService(ApiClient apiClient, ITokenStore tokenStore)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
    }

    public async Task<AuthResult<IdentityResultDto>> RegisterAsync(Auth.Models.RegisterDto request, CancellationToken ct = default)
    {
        try
        {
            var kiotaDto = new Client.Models.RegisterDto
            {
                Email = request.Email,
                Password = request.Password
            };

            await _apiClient.Api.V1.Auth.Register.PostAsync(kiotaDto, cancellationToken: ct);
            return AuthResult<IdentityResultDto>.Ok(new IdentityResultDto(true, Array.Empty<string>()), "Usuário registrado com sucesso.");
        }
        catch (Client.Models.IdentityResultResponse ex)
        {
            var errors = ex.Errors?.ToList() ?? new List<string>();
            return AuthResult<IdentityResultDto>.Fail(string.Join("; ", errors), errors, ex.ResponseStatusCode);
        }
        catch (ApiException ex)
        {
            return AuthResult<IdentityResultDto>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<IdentityResultDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<TokenResponseDto>> LoginAsync(Auth.Models.LoginDto request, CancellationToken ct = default)
    {
        try
        {
            var kiotaDto = new Client.Models.LoginDto
            {
                Email = request.Email,
                Password = request.Password,
                TwoFactorCode = request.TwoFactorCode,
                TwoFactorRecoveryCode = request.TwoFactorRecoveryCode
            };

            var tokenResponse = await _apiClient.Api.V1.Auth.Login.PostAsync(kiotaDto, cancellationToken: ct);
            if (tokenResponse is not null && !string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                await _tokenStore.SaveTokensAsync(tokenResponse.AccessToken, tokenResponse.RefreshToken);
                var dto = new TokenResponseDto(
                    tokenResponse.TokenType ?? "Bearer",
                    tokenResponse.AccessToken,
                    tokenResponse.ExpiresIn ?? 3600,
                    tokenResponse.RefreshToken ?? string.Empty);

                return AuthResult<TokenResponseDto>.Ok(dto, "Login efetuado com sucesso.");
            }

            return AuthResult<TokenResponseDto>.Fail("Falha ao autenticar usuário.");
        }
        catch (Client.Models.ResponseBaseOfTokenResponse ex)
        {
            return AuthResult<TokenResponseDto>.Fail(ex.Message ?? "Credenciais inválidas.", statusCode: ex.ResponseStatusCode);
        }
        catch (ApiException ex)
        {
            return AuthResult<TokenResponseDto>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<TokenResponseDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<TokenResponseDto>> RefreshTokenAsync(Auth.Models.RefreshDto request, CancellationToken ct = default)
    {
        try
        {
            var kiotaDto = new Client.Models.RefreshDto
            {
                RefreshToken = request.RefreshToken
            };

            var tokenResponse = await _apiClient.Api.V1.Auth.Refresh.PostAsync(kiotaDto, cancellationToken: ct);
            if (tokenResponse is not null && !string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                await _tokenStore.SaveTokensAsync(tokenResponse.AccessToken, tokenResponse.RefreshToken);
                var dto = new TokenResponseDto(
                    tokenResponse.TokenType ?? "Bearer",
                    tokenResponse.AccessToken,
                    tokenResponse.ExpiresIn ?? 3600,
                    tokenResponse.RefreshToken ?? string.Empty);

                return AuthResult<TokenResponseDto>.Ok(dto, "Token renovado com sucesso.");
            }

            return AuthResult<TokenResponseDto>.Fail("Falha ao renovar token.");
        }
        catch (Client.Models.ResponseBaseOfTokenResponse ex)
        {
            return AuthResult<TokenResponseDto>.Fail(ex.Message ?? "Falha ao renovar token.", statusCode: ex.ResponseStatusCode);
        }
        catch (ApiException ex)
        {
            return AuthResult<TokenResponseDto>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
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
            await _apiClient.Api.V1.Auth.Revoke.PostAsync(cancellationToken: ct);
            await _tokenStore.ClearTokensAsync();
            return AuthResult.Ok("Sessão revogada com sucesso.");
        }
        catch (Client.Models.ResponseBaseOfboolean ex)
        {
            return AuthResult.Fail(ex.Message ?? "Falha ao revogar sessão.", statusCode: ex.ResponseStatusCode);
        }
        catch (ApiException ex)
        {
            return AuthResult.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
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
            await _apiClient.Api.V1.Auth.ConfirmEmail.GetAsync(reqConfig =>
            {
                reqConfig.QueryParameters.UserId = userId;
                reqConfig.QueryParameters.Code = code;
                reqConfig.QueryParameters.ChangedEmail = changedEmail;
            }, cancellationToken: ct);

            return AuthResult<bool>.Ok(true, "Email confirmado com sucesso.");
        }
        catch (Client.Models.ResponseBaseOfboolean ex)
        {
            return AuthResult<bool>.Fail(ex.Message ?? "Falha ao confirmar email.", statusCode: ex.ResponseStatusCode);
        }
        catch (ApiException ex)
        {
            return AuthResult<bool>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<bool>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<IdentityResultDto>> ResendConfirmationEmailAsync(Auth.Models.EmailDto request, CancellationToken ct = default)
    {
        try
        {
            var kiotaDto = new Client.Models.EmailDto { Email = request.Email };
            var result = await _apiClient.Api.V1.Auth.ResendConfirmationEmail.PostAsync(kiotaDto, cancellationToken: ct);
            var errors = result?.Errors?.ToList() ?? new List<string>();
            var succeeded = result?.Succeeded ?? false;
            var dto = new IdentityResultDto(succeeded, errors);

            return succeeded
                ? AuthResult<IdentityResultDto>.Ok(dto, "Email de confirmação reenviado.")
                : AuthResult<IdentityResultDto>.Fail(string.Join("; ", errors), errors);
        }
        catch (Client.Models.IdentityResultResponse ex)
        {
            var errors = ex.Errors?.ToList() ?? new List<string>();
            return AuthResult<IdentityResultDto>.Fail(string.Join("; ", errors), errors, ex.ResponseStatusCode);
        }
        catch (ApiException ex)
        {
            return AuthResult<IdentityResultDto>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<IdentityResultDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<IdentityResultDto>> ForgotPasswordAsync(Auth.Models.EmailDto request, CancellationToken ct = default)
    {
        try
        {
            var kiotaDto = new Client.Models.EmailDto { Email = request.Email };
            await _apiClient.Api.V1.Auth.ForgotPassword.PostAsync(kiotaDto, cancellationToken: ct);
            return AuthResult<IdentityResultDto>.Ok(new IdentityResultDto(true, Array.Empty<string>()), "Instruções de recuperação enviadas.");
        }
        catch (Client.Models.IdentityResultResponse ex)
        {
            var errors = ex.Errors?.ToList() ?? new List<string>();
            return AuthResult<IdentityResultDto>.Fail(string.Join("; ", errors), errors, ex.ResponseStatusCode);
        }
        catch (ApiException ex)
        {
            return AuthResult<IdentityResultDto>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<IdentityResultDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<IdentityResultDto>> ResetPasswordAsync(Auth.Models.ResetPasswordDto request, CancellationToken ct = default)
    {
        try
        {
            var kiotaDto = new Client.Models.ResetPasswordDto
            {
                Email = request.Email,
                ResetCode = request.ResetCode,
                NewPassword = request.NewPassword
            };

            await _apiClient.Api.V1.Auth.ResetPassword.PostAsync(kiotaDto, cancellationToken: ct);
            return AuthResult<IdentityResultDto>.Ok(new IdentityResultDto(true, Array.Empty<string>()), "Senha redefinida com sucesso.");
        }
        catch (Client.Models.IdentityResultResponse ex)
        {
            var errors = ex.Errors?.ToList() ?? new List<string>();
            return AuthResult<IdentityResultDto>.Fail(string.Join("; ", errors), errors, ex.ResponseStatusCode);
        }
        catch (ApiException ex)
        {
            return AuthResult<IdentityResultDto>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
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
            var info = await _apiClient.Api.V1.Auth.Manage.Info.GetAsync(cancellationToken: ct);
            if (info is not null)
            {
                var dto = new IdentityInfoDto(
                    info.Email ?? string.Empty,
                    info.IsEmailConfirmed ?? false,
                    info.DisplayName,
                    info.FirstName,
                    info.SurName);

                return AuthResult<IdentityInfoDto>.Ok(dto);
            }

            return AuthResult<IdentityInfoDto>.Fail("Informações de usuário não encontradas.", statusCode: 404);
        }
        catch (Client.Models.ResponseBaseOfIdentityInfoResponse ex)
        {
            return AuthResult<IdentityInfoDto>.Fail(ex.Message ?? "Usuário não encontrado.", statusCode: ex.ResponseStatusCode);
        }
        catch (ApiException ex)
        {
            return AuthResult<IdentityInfoDto>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<IdentityInfoDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<IdentityResultDto>> UpdateIdentityInfoAsync(Auth.Models.InfoDto request, CancellationToken ct = default)
    {
        try
        {
            var kiotaDto = new Client.Models.InfoDto
            {
                NewEmail = request.NewEmail,
                NewPassword = request.NewPassword,
                OldPassword = request.OldPassword
            };

            await _apiClient.Api.V1.Auth.Manage.Info.PostAsync(kiotaDto, cancellationToken: ct);
            return AuthResult<IdentityResultDto>.Ok(new IdentityResultDto(true, Array.Empty<string>()), "Informações atualizadas.");
        }
        catch (Client.Models.IdentityResultResponse ex)
        {
            var errors = ex.Errors?.ToList() ?? new List<string>();
            return AuthResult<IdentityResultDto>.Fail(string.Join("; ", errors), errors, ex.ResponseStatusCode);
        }
        catch (ApiException ex)
        {
            return AuthResult<IdentityResultDto>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<IdentityResultDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<TwoFactorResponseDto>> ConfigureTwoFactorAsync(Auth.Models.TwoFactorDto request, CancellationToken ct = default)
    {
        try
        {
            var kiotaDto = new Client.Models.TwoFactorDto
            {
                Enable = request.Enable,
                TwoFactorCode = request.TwoFactorCode,
                ResetRecoveryCodes = request.ResetRecoveryCodes,
                ResetSharedKey = request.ResetSharedKey,
                ForgetMachine = request.ForgetMachine
            };

            var twoFa = await _apiClient.Api.V1.Auth.Manage.Twofa.PostAsync(kiotaDto, cancellationToken: ct);
            if (twoFa is not null)
            {
                var dto = new TwoFactorResponseDto(
                    twoFa.SharedKey,
                    twoFa.RecoveryCodesLeft ?? 0,
                    twoFa.RecoveryCodes?.ToList(),
                    twoFa.IsTwoFactorEnabled ?? false,
                    twoFa.IsMachineRemembered ?? false);

                return AuthResult<TwoFactorResponseDto>.Ok(dto, "2FA configurado com sucesso.");
            }

            return AuthResult<TwoFactorResponseDto>.Fail("Falha ao configurar 2FA.");
        }
        catch (Client.Models.IdentityResultResponse ex)
        {
            var errors = ex.Errors?.ToList() ?? new List<string>();
            return AuthResult<TwoFactorResponseDto>.Fail(string.Join("; ", errors), errors, ex.ResponseStatusCode);
        }
        catch (ApiException ex)
        {
            return AuthResult<TwoFactorResponseDto>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
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
            var response = await _apiClient.Api.V1.Auth.Roles.GetAsync(cancellationToken: ct);
            var list = response?.Value?.Select(r => new RoleCatalogItemDto(r.RoleName ?? string.Empty, r.Rank ?? 0)).ToList()
                ?? new List<RoleCatalogItemDto>();

            return AuthResult<IReadOnlyList<RoleCatalogItemDto>>.Ok(list);
        }
        catch (ApiException ex)
        {
            return AuthResult<IReadOnlyList<RoleCatalogItemDto>>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
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
            var response = await _apiClient.Api.V1.Auth.Users[userId].Roles.GetAsync(cancellationToken: ct);
            if (response?.Value is not null)
            {
                var dto = new UserRolesDto(
                    response.Value.UserId ?? userId,
                    response.Value.Roles?.ToList() ?? new List<string>());

                return AuthResult<UserRolesDto>.Ok(dto);
            }

            return AuthResult<UserRolesDto>.Fail(response?.Message ?? "Roles não encontradas.", statusCode: 404);
        }
        catch (ApiException ex)
        {
            return AuthResult<UserRolesDto>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<UserRolesDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<bool>> AssignUserRoleAsync(string userId, Auth.Models.AssignRoleDto request, CancellationToken ct = default)
    {
        try
        {
            var kiotaDto = new Client.Models.AssignRoleDto { RoleName = request.RoleName };
            var response = await _apiClient.Api.V1.Auth.Users[userId].Roles.PostAsync(kiotaDto, cancellationToken: ct);
            if (response?.Success == true)
            {
                return AuthResult<bool>.Ok(true, response.Message ?? "Role atribuída.");
            }

            return AuthResult<bool>.Fail(response?.Message ?? "Falha ao atribuir role.");
        }
        catch (ApiException ex)
        {
            return AuthResult<bool>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
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
            var response = await _apiClient.Api.V1.Auth.Users[userId].Roles[roleName].DeleteAsync(cancellationToken: ct);
            if (response?.Success == true)
            {
                return AuthResult<bool>.Ok(true, response.Message ?? "Role removida.");
            }

            return AuthResult<bool>.Fail(response?.Message ?? "Falha ao remover role.");
        }
        catch (ApiException ex)
        {
            return AuthResult<bool>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
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
            var response = await _apiClient.Api.V1.Auth.Permissions.GetAsync(cancellationToken: ct);
            if (response?.Value is not null)
            {
                var permissions = response.Value.Permissions?.ToList() ?? new List<string>();
                var groups = new Dictionary<string, IReadOnlyList<string>>();

                if (response.Value.Groups?.AdditionalData is not null)
                {
                    foreach (var kvp in response.Value.Groups.AdditionalData)
                    {
                        if (kvp.Value is IEnumerable list)
                        {
                            var items = new List<string>();
                            foreach (var item in list)
                            {
                                if (item is not null)
                                {
                                    items.Add(item.ToString()!);
                                }
                            }
                            groups[kvp.Key] = items;
                        }
                    }
                }

                return AuthResult<PermissionCatalogDto>.Ok(new PermissionCatalogDto(permissions, groups));
            }

            return AuthResult<PermissionCatalogDto>.Fail("Catálogo de permissões não disponível.");
        }
        catch (ApiException ex)
        {
            return AuthResult<PermissionCatalogDto>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
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
            var response = await _apiClient.Api.V1.Auth.Users[userId].Permissions.GetAsync(cancellationToken: ct);
            if (response?.Value is not null)
            {
                var dto = new UserPermissionsDto(
                    response.Value.UserId ?? userId,
                    response.Value.EffectivePermissions?.ToList() ?? new List<string>(),
                    response.Value.DirectGrants?.ToList() ?? new List<string>(),
                    response.Value.ExplicitRevocations?.ToList() ?? new List<string>(),
                    response.Value.AssignedGroups?.ToList() ?? new List<string>());

                return AuthResult<UserPermissionsDto>.Ok(dto);
            }

            return AuthResult<UserPermissionsDto>.Fail(response?.Message ?? "Permissões não encontradas.");
        }
        catch (ApiException ex)
        {
            return AuthResult<UserPermissionsDto>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<UserPermissionsDto>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<bool>> GrantUserPermissionAsync(string userId, Auth.Models.GrantPermissionDto request, CancellationToken ct = default)
    {
        try
        {
            var kiotaDto = new Client.Models.GrantPermissionDto { PermissionName = request.PermissionName };
            var response = await _apiClient.Api.V1.Auth.Users[userId].Permissions.Grant.PostAsync(kiotaDto, cancellationToken: ct);
            if (response?.Success == true)
            {
                return AuthResult<bool>.Ok(true, response.Message ?? "Permissão concedida.");
            }

            return AuthResult<bool>.Fail(response?.Message ?? "Falha ao conceder permissão.");
        }
        catch (ApiException ex)
        {
            return AuthResult<bool>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<bool>.Fail($"Erro na requisição: {ex.Message}");
        }
    }

    public async Task<AuthResult<bool>> RevokeUserPermissionAsync(string userId, Auth.Models.RevokePermissionDto request, CancellationToken ct = default)
    {
        try
        {
            var kiotaDto = new Client.Models.RevokePermissionDto { PermissionName = request.PermissionName };
            var response = await _apiClient.Api.V1.Auth.Users[userId].Permissions.Revoke.PostAsync(kiotaDto, cancellationToken: ct);
            if (response?.Success == true)
            {
                return AuthResult<bool>.Ok(true, response.Message ?? "Permissão revogada.");
            }

            return AuthResult<bool>.Fail(response?.Message ?? "Falha ao revogar permissão.");
        }
        catch (ApiException ex)
        {
            return AuthResult<bool>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
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
            var response = await _apiClient.Api.V1.Auth.Users[userId].Permissions[permissionName].DeleteAsync(cancellationToken: ct);
            if (response?.Success == true)
            {
                return AuthResult<bool>.Ok(true, response.Message ?? "Override de permissão limpo.");
            }

            return AuthResult<bool>.Fail(response?.Message ?? "Falha ao limpar override de permissão.");
        }
        catch (ApiException ex)
        {
            return AuthResult<bool>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
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
            var kiotaDto = new Client.Models.AssignPermissionGroupDto { GroupName = request.GroupName };
            var response = await _apiClient.Api.V1.Auth.Users[userId].Groups.PostAsync(kiotaDto, cancellationToken: ct);
            if (response?.Success == true)
            {
                return AuthResult<bool>.Ok(true, response.Message ?? "Grupo associado.");
            }

            return AuthResult<bool>.Fail(response?.Message ?? "Falha ao associar grupo.");
        }
        catch (ApiException ex)
        {
            return AuthResult<bool>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
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
            var response = await _apiClient.Api.V1.Auth.Users[userId].Groups[groupName].DeleteAsync(cancellationToken: ct);
            if (response?.Success == true)
            {
                return AuthResult<bool>.Ok(true, response.Message ?? "Grupo desassociado.");
            }

            return AuthResult<bool>.Fail(response?.Message ?? "Falha ao desassociar grupo.");
        }
        catch (ApiException ex)
        {
            return AuthResult<bool>.Fail(ex.Message, statusCode: ex.ResponseStatusCode);
        }
        catch (Exception ex)
        {
            return AuthResult<bool>.Fail($"Erro na requisição: {ex.Message}");
        }
    }
}
