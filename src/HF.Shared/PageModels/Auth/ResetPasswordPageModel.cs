using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Auth;
using HF.Shared.Auth.Models;

namespace HF.Shared.PageModels.Auth;

public partial class ResetPasswordPageModel : PageModelBase
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ResetCode { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmNewPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsPasswordReset { get; set; } = false;

    public ResetPasswordPageModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Redefinir Senha";
    }

    public async Task<bool> ResetPasswordAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "O email é obrigatório.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(ResetCode))
        {
            ErrorMessage = "O código de redefinição é obrigatório.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            ErrorMessage = "A nova senha é obrigatória.";
            return false;
        }

        if (NewPassword != ConfirmNewPassword)
        {
            ErrorMessage = "As senhas não coincidem.";
            return false;
        }

        try
        {
            IsBusy = true;
            var request = new ResetPasswordDto(Email, ResetCode, NewPassword);
            var result = await _authService.ResetPasswordAsync(request, ct);

            if (result.Succeeded)
            {
                IsPasswordReset = true;
                SuccessMessage = "Senha redefinida com êxito! Faça login com a nova senha.";
                return true;
            }

            ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                ? result.Message
                : string.Join("; ", result.Errors);

            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro inesperado: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

