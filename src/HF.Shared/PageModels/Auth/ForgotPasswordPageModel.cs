using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Auth;
using HF.Shared.Auth.Models;

namespace HF.Shared.PageModels.Auth;

public partial class ForgotPasswordPageModel : PageModelBase
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool EmailSent { get; set; } = false;

    public ForgotPasswordPageModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Esqueci Minha Senha";
    }

    public async Task<bool> SendForgotPasswordAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "O email é obrigatório.";
            return false;
        }

        try
        {
            IsBusy = true;
            var request = new EmailDto(Email);
            var result = await _authService.ForgotPasswordAsync(request, ct);

            if (result.Succeeded)
            {
                EmailSent = true;
                SuccessMessage = "Instruções de recuperação foram enviadas para seu email caso ele esteja cadastrado.";
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

