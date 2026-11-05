using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Auth;
using HF.Shared.Auth.Models;

namespace HF.Shared.PageModels.Auth;

public partial class ManageProfilePageModel : PageModelBase
{
    private readonly IAuthService _authService;

    // Profile info
    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsEmailConfirmed { get; set; } = false;

    [ObservableProperty]
    public partial string? DisplayName { get; set; }

    [ObservableProperty]
    public partial string? FirstName { get; set; }

    [ObservableProperty]
    public partial string? SurName { get; set; }

    // Update info inputs
    [ObservableProperty]
    public partial string? NewEmail { get; set; }

    [ObservableProperty]
    public partial string? NewPassword { get; set; }

    [ObservableProperty]
    public partial string OldPassword { get; set; } = string.Empty;

    // 2FA state and inputs
    [ObservableProperty]
    public partial bool IsTwoFactorEnabled { get; set; } = false;

    [ObservableProperty]
    public partial string? SharedKey { get; set; }

    [ObservableProperty]
    public partial int RecoveryCodesLeft { get; set; }

    [ObservableProperty]
    public partial IReadOnlyCollection<string>? RecoveryCodes { get; set; }

    [ObservableProperty]
    public partial bool IsMachineRemembered { get; set; } = false;

    [ObservableProperty]
    public partial string? TwoFactorCodeInput { get; set; }

    // Refresh token input
    [ObservableProperty]
    public partial string? RefreshTokenInput { get; set; }

    // Status feedback
    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    public ManageProfilePageModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Meu Perfil e Segurança";
    }

    public async Task<bool> LoadInfoAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        try
        {
            IsBusy = true;
            var result = await _authService.GetIdentityInfoAsync(ct);
            if (result.Succeeded && result.Data is not null)
            {
                Email = result.Data.Email;
                IsEmailConfirmed = result.Data.IsEmailConfirmed;
                DisplayName = result.Data.DisplayName;
                FirstName = result.Data.FirstName;
                SurName = result.Data.SurName;
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar dados do usuário: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> UpdateInfoAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(OldPassword))
        {
            ErrorMessage = "A senha atual é necessária para confirmar as alterações.";
            return false;
        }

        try
        {
            IsBusy = true;
            var request = new InfoDto(NewEmail, NewPassword, OldPassword);
            var result = await _authService.UpdateIdentityInfoAsync(request, ct);

            if (result.Succeeded)
            {
                SuccessMessage = "Informações atualizadas com sucesso!";
                OldPassword = string.Empty;
                NewPassword = string.Empty;
                NewEmail = string.Empty;
                await LoadInfoAsync(ct);
                return true;
            }

            ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                ? result.Message
                : string.Join("; ", result.Errors);

            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao atualizar dados: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> ConfigureTwoFactorAsync(
        bool? enable = null,
        bool resetRecoveryCodes = false,
        bool resetSharedKey = false,
        bool forgetMachine = false,
        CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            IsBusy = true;
            var request = new TwoFactorDto(
                enable,
                TwoFactorCodeInput,
                resetRecoveryCodes,
                resetSharedKey,
                forgetMachine);

            var result = await _authService.ConfigureTwoFactorAsync(request, ct);

            if (result.Succeeded && result.Data is not null)
            {
                IsTwoFactorEnabled = result.Data.IsTwoFactorEnabled;
                SharedKey = result.Data.SharedKey;
                RecoveryCodesLeft = result.Data.RecoveryCodesLeft;
                RecoveryCodes = result.Data.RecoveryCodes;
                IsMachineRemembered = result.Data.IsMachineRemembered;
                TwoFactorCodeInput = null;
                SuccessMessage = "Configuração de 2FA atualizada com sucesso!";
                return true;
            }

            ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                ? result.Message
                : string.Join("; ", result.Errors);

            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao configurar 2FA: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> RefreshTokensAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(RefreshTokenInput))
        {
            ErrorMessage = "Informe o refresh token para renovação.";
            return false;
        }

        try
        {
            IsBusy = true;
            var request = new RefreshDto(RefreshTokenInput);
            var result = await _authService.RefreshTokenAsync(request, ct);

            if (result.Succeeded && result.Data is not null)
            {
                RefreshTokenInput = result.Data.RefreshToken;
                SuccessMessage = "Token de autenticação renovado com êxito!";
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao renovar token: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> RevokeTokenAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            IsBusy = true;
            var result = await _authService.RevokeTokenAsync(ct);

            if (result.Succeeded)
            {
                SuccessMessage = "Sessão revogada com sucesso.";
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao revogar sessão: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

