using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Auth;
using HF.Shared.Auth.Models;

namespace HF.Shared.PageModels.Auth;

public partial class ManageRolesPageModel : PageModelBase
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    public partial string TargetUserId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedRoleToAssign { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IReadOnlyList<RoleCatalogItemDto> CatalogRoles { get; set; } = Array.Empty<RoleCatalogItemDto>();

    [ObservableProperty]
    public partial IReadOnlyList<string> UserRoles { get; set; } = Array.Empty<string>();

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    public ManageRolesPageModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Gerenciamento de Roles";
    }

    public async Task<bool> LoadCatalogRolesAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        try
        {
            IsBusy = true;
            var result = await _authService.ListCatalogRolesAsync(ct);
            if (result.Succeeded && result.Data is not null)
            {
                CatalogRoles = result.Data;
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar catálogo de roles: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> LoadUserRolesAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(TargetUserId))
        {
            ErrorMessage = "Informe o ID do usuário para consultar suas roles.";
            return false;
        }

        try
        {
            IsBusy = true;
            var result = await _authService.GetUserRolesAsync(TargetUserId, ct);
            if (result.Succeeded && result.Data is not null)
            {
                UserRoles = result.Data.Roles;
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar roles do usuário: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> AssignRoleAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(TargetUserId))
        {
            ErrorMessage = "Informe o ID do usuário.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(SelectedRoleToAssign))
        {
            ErrorMessage = "Selecione a role a ser atribuída.";
            return false;
        }

        try
        {
            IsBusy = true;
            var request = new AssignRoleDto(SelectedRoleToAssign);
            var result = await _authService.AssignUserRoleAsync(TargetUserId, request, ct);

            if (result.Succeeded)
            {
                SuccessMessage = $"Role '{SelectedRoleToAssign}' atribuída com sucesso ao usuário!";
                SelectedRoleToAssign = string.Empty;
                await LoadUserRolesAsync(ct);
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao atribuir role: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> RemoveRoleAsync(string roleName, CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(TargetUserId))
        {
            ErrorMessage = "Informe o ID do usuário.";
            return false;
        }

        try
        {
            IsBusy = true;
            var result = await _authService.RemoveUserRoleAsync(TargetUserId, roleName, ct);

            if (result.Succeeded)
            {
                SuccessMessage = $"Role '{roleName}' removida com sucesso do usuário!";
                await LoadUserRolesAsync(ct);
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao remover role: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

