using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Auth;
using HF.Shared.Auth.Models;

namespace HF.Shared.PageModels.Auth;

public partial class RegisterPageModel : PageModelBase
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsRegistered { get; set; } = false;

    public RegisterPageModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Criar Nova Conta";
    }

    public async Task<bool> RegisterAsync(CancellationToken ct = default)
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

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "As senhas não coincidem.";
            return false;
        }

        try
        {
            IsBusy = true;
            var request = new RegisterDto(Email, Password);
            var result = await _authService.RegisterAsync(request, ct);

            if (result.Succeeded)
            {
                IsRegistered = true;
                SuccessMessage = "Cadastro realizado com sucesso! Verifique seu email para ativá-lo.";
                return true;
            }

            ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                ? result.Message
                : string.Join("; ", result.Errors);

            if (string.IsNullOrWhiteSpace(ErrorMessage))
            {
                ErrorMessage = "Falha ao registrar novo usuário.";
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

