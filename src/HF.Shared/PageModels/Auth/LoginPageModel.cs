using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Auth;
using HF.Shared.Auth.Models;

namespace HF.Shared.PageModels.Auth;

public partial class LoginPageModel : PageModelBase
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? TwoFactorCode { get; set; }

    [ObservableProperty]
    public partial string? TwoFactorRecoveryCode { get; set; }

    [ObservableProperty]
    public partial bool ShowTwoFactor { get; set; } = false;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsLoggedIn { get; set; } = false;

    public LoginPageModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Entrar no Horizon";
    }

    public async Task<bool> LoginAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "O email é obrigatório.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "A senha é obrigatória.";
            return false;
        }

        try
        {
            IsBusy = true;
            var request = new LoginDto(Email, Password, TwoFactorCode, TwoFactorRecoveryCode);
            var result = await _authService.LoginAsync(request, ct);

            if (result.Succeeded)
            {
                IsLoggedIn = true;
                SuccessMessage = "Autenticação realizada com sucesso!";
                return true;
            }

            ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                ? result.Message
                : string.Join("; ", result.Errors);

            if (string.IsNullOrWhiteSpace(ErrorMessage))
            {
                ErrorMessage = "Falha ao realizar login. Verifique suas credenciais.";
            }

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

