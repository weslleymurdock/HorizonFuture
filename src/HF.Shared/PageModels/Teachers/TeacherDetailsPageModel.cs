using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Teachers;
using HF.Shared.Teachers.Models;

namespace HF.Shared.PageModels.Teachers;

/// <summary>
/// PageModel para a tela de detalhes de professor (/teachers/{id}).
/// </summary>
public partial class TeacherDetailsPageModel : PageModelBase
{
    private readonly ITeacherService _teacherService;

    [ObservableProperty]
    public partial TeacherItemDto? Teacher { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsForbidden { get; set; }

    [ObservableProperty]
    public partial bool IsUnauthorized { get; set; }

    [ObservableProperty]
    public partial bool IsNotFound { get; set; }

    [ObservableProperty]
    public partial bool IsDeleting { get; set; }

    public TeacherDetailsPageModel(ITeacherService teacherService)
    {
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        Title = "Detalhes do Professor";
    }

    /// <summary>
    /// Carrega os detalhes do professor pelo identificador único.
    /// </summary>
    public async Task<bool> LoadTeacherAsync(string id, CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsForbidden = false;
        IsUnauthorized = false;
        IsNotFound = false;
        Teacher = null;

        if (string.IsNullOrWhiteSpace(id))
        {
            ErrorMessage = "Identificador do professor é obrigatório.";
            IsNotFound = true;
            return false;
        }

        try
        {
            IsBusy = true;
            var result = await _teacherService.GetByIdAsync(id, ct);

            if (result.Succeeded && result.Data is not null)
            {
                Teacher = result.Data;
                Title = $"Professor - {Teacher.EmployeeNumber}";
                return true;
            }

            ErrorMessage = result.Message;
            if (result.StatusCode == 404)
            {
                IsNotFound = true;
            }
            else if (result.StatusCode == 403)
            {
                IsForbidden = true;
            }
            else if (result.StatusCode == 401)
            {
                IsUnauthorized = true;
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

    /// <summary>
    /// Exclui o professor atualmente carregado.
    /// </summary>
    public async Task<bool> DeleteTeacherAsync(CancellationToken ct = default)
    {
        if (Teacher is null || string.IsNullOrWhiteSpace(Teacher.Id))
        {
            ErrorMessage = "Nenhum professor selecionado para exclusão.";
            return false;
        }

        ErrorMessage = null;
        SuccessMessage = null;
        IsForbidden = false;
        IsUnauthorized = false;

        try
        {
            IsDeleting = true;
            var result = await _teacherService.DeleteAsync(Teacher.Id, ct);

            if (result.Succeeded)
            {
                SuccessMessage = "Professor excluído com sucesso.";
                return true;
            }

            ErrorMessage = result.Message;
            if (result.StatusCode == 403)
            {
                IsForbidden = true;
            }
            else if (result.StatusCode == 401)
            {
                IsUnauthorized = true;
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
            IsDeleting = false;
        }
    }
}

