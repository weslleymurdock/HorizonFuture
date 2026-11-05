using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Auth;
using HF.Shared.Auth.Models;

namespace HF.Shared.PageModels.Auth;

public partial class ManagePermissionsPageModel : PageModelBase
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    public partial string TargetUserId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedPermissionToGrant { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedPermissionToRevoke { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedGroupToAssign { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IReadOnlyList<string> CatalogPermissions { get; set; } = Array.Empty<string>();

    [ObservableProperty]
    public partial IReadOnlyDictionary<string, IReadOnlyList<string>> CatalogGroups { get; set; } = new Dictionary<string, IReadOnlyList<string>>();

    [ObservableProperty]
    public partial IReadOnlyList<string> EffectivePermissions { get; set; } = Array.Empty<string>();

    [ObservableProperty]
    public partial IReadOnlyList<string> DirectGrants { get; set; } = Array.Empty<string>();

    [ObservableProperty]
    public partial IReadOnlyList<string> ExplicitRevocations { get; set; } = Array.Empty<string>();

    [ObservableProperty]
    public partial IReadOnlyList<string> AssignedGroups { get; set; } = Array.Empty<string>();

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    public ManagePermissionsPageModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Gerenciamento de Permissões e Grupos";
    }

    public async Task<bool> LoadCatalogPermissionsAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        try
        {
            IsBusy = true;
            var result = await _authService.ListCatalogPermissionsAsync(ct);
            if (result.Succeeded && result.Data is not null)
            {
                CatalogPermissions = result.Data.Permissions;
                CatalogGroups = result.Data.Groups;
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar catálogo de permissões: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> LoadUserPermissionsAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(TargetUserId))
        {
            ErrorMessage = "Informe o ID do usuário para consultar suas permissões.";
            return false;
        }

        try
        {
            IsBusy = true;
            var result = await _authService.GetUserPermissionsAsync(TargetUserId, ct);
            if (result.Succeeded && result.Data is not null)
            {
                EffectivePermissions = result.Data.EffectivePermissions;
                DirectGrants = result.Data.DirectGrants;
                ExplicitRevocations = result.Data.ExplicitRevocations;
                AssignedGroups = result.Data.AssignedGroups;
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar permissões do usuário: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> GrantPermissionAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(TargetUserId))
        {
            ErrorMessage = "Informe o ID do usuário.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(SelectedPermissionToGrant))
        {
            ErrorMessage = "Selecione a permissão a ser concedida.";
            return false;
        }

        try
        {
            IsBusy = true;
            var request = new GrantPermissionDto(SelectedPermissionToGrant);
            var result = await _authService.GrantUserPermissionAsync(TargetUserId, request, ct);

            if (result.Succeeded)
            {
                SuccessMessage = $"Permissão '{SelectedPermissionToGrant}' concedida com sucesso!";
                SelectedPermissionToGrant = string.Empty;
                await LoadUserPermissionsAsync(ct);
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao conceder permissão: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> RevokePermissionAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(TargetUserId))
        {
            ErrorMessage = "Informe o ID do usuário.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(SelectedPermissionToRevoke))
        {
            ErrorMessage = "Selecione a permissão a ser revogada.";
            return false;
        }

        try
        {
            IsBusy = true;
            var request = new RevokePermissionDto(SelectedPermissionToRevoke);
            var result = await _authService.RevokeUserPermissionAsync(TargetUserId, request, ct);

            if (result.Succeeded)
            {
                SuccessMessage = $"Permissão '{SelectedPermissionToRevoke}' revogada com sucesso!";
                SelectedPermissionToRevoke = string.Empty;
                await LoadUserPermissionsAsync(ct);
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao revogar permissão: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> ClearPermissionAsync(string permissionName, CancellationToken ct = default)
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
            var result = await _authService.ClearUserPermissionAsync(TargetUserId, permissionName, ct);

            if (result.Succeeded)
            {
                SuccessMessage = $"Override da permissão '{permissionName}' limpo com sucesso!";
                await LoadUserPermissionsAsync(ct);
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao limpar override de permissão: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> AssignGroupAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(TargetUserId))
        {
            ErrorMessage = "Informe o ID do usuário.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(SelectedGroupToAssign))
        {
            ErrorMessage = "Selecione o grupo a ser atribuído.";
            return false;
        }

        try
        {
            IsBusy = true;
            var request = new AssignPermissionGroupDto(SelectedGroupToAssign);
            var result = await _authService.AssignUserPermissionGroupAsync(TargetUserId, request, ct);

            if (result.Succeeded)
            {
                SuccessMessage = $"Grupo '{SelectedGroupToAssign}' atribuído com sucesso!";
                SelectedGroupToAssign = string.Empty;
                await LoadUserPermissionsAsync(ct);
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao atribuir grupo: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> RemoveGroupAsync(string groupName, CancellationToken ct = default)
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
            var result = await _authService.RemoveUserPermissionGroupAsync(TargetUserId, groupName, ct);

            if (result.Succeeded)
            {
                SuccessMessage = $"Grupo '{groupName}' removido com sucesso!";
                await LoadUserPermissionsAsync(ct);
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao remover grupo: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

