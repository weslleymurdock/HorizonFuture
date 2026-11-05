using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Auth;
using HF.Shared.Auth.Models;

namespace HF.Shared.PageModels.Auth;

public partial class ConfirmEmailPageModel : PageModelBase
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    public partial string UserId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Code { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ChangedEmail { get; set; }

    [ObservableProperty]
    public partial string ResendEmail { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsConfirmed { get; set; } = false;

    public ConfirmEmailPageModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Confirmação de Email";
    }

    public async Task<bool> ConfirmEmailAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(UserId))
        {
            ErrorMessage = "O identificador do usuário é obrigatório.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Code))
        {
            ErrorMessage = "O código de confirmação é obrigatório.";
            return false;
        }

        try
        {
            IsBusy = true;
            var result = await _authService.ConfirmEmailAsync(UserId, Code, ChangedEmail, ct);

            if (result.Succeeded && result.Data)
            {
                IsConfirmed = true;
                SuccessMessage = "Email confirmado com sucesso! Sua conta está ativada.";
                return true;
            }

            ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                ? result.Message
                : "Falha na confirmação de email. O código pode ser inválido ou expirado.";

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

    public async Task<bool> ResendConfirmationEmailAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(ResendEmail))
        {
            ErrorMessage = "Informe o email para reenviar o link de confirmação.";
            return false;
        }

        try
        {
            IsBusy = true;
            var request = new EmailDto(ResendEmail);
            var result = await _authService.ResendConfirmationEmailAsync(request, ct);

            if (result.Succeeded)
            {
                SuccessMessage = "Email de confirmação reenviado com sucesso.";
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

