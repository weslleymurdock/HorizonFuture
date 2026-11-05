using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Courses;
using HF.Shared.Courses.Models;

namespace HF.Shared.PageModels.Courses;

/// <summary>
/// PageModel para visualização detalhada de um curso (/courses/{id}).
/// </summary>
public partial class CourseDetailsPageModel : PageModelBase
{
    private readonly ICourseService _courseService;

    [ObservableProperty]
    public partial string CourseId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial CourseItemDto? Course { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsNotFound { get; set; }

    [ObservableProperty]
    public partial bool IsForbidden { get; set; }

    [ObservableProperty]
    public partial bool IsUnauthorized { get; set; }

    [ObservableProperty]
    public partial bool IsDeleting { get; set; }

    [ObservableProperty]
    public partial bool IsDeleted { get; set; }

    public CourseDetailsPageModel(ICourseService courseService)
    {
        _courseService = courseService;
        Title = "Detalhes do Curso";
    }

    /// <summary>
    /// Carrega as informações completas do curso pelo identificador único.
    /// </summary>
    public async Task<bool> LoadCourseAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            CourseId = string.Empty;
            Course = null;
            IsNotFound = true;
            ErrorMessage = "Identificador do curso não informado.";
            return false;
        }

        CourseId = id;
        ErrorMessage = null;
        SuccessMessage = null;
        IsNotFound = false;
        IsForbidden = false;
        IsUnauthorized = false;
        IsDeleted = false;

        try
        {
            IsBusy = true;
            var result = await _courseService.GetByIdAsync(id, ct);

            if (result.Succeeded && result.Data is not null)
            {
                Course = result.Data;
                Title = $"Curso: {result.Data.Name}";
                return true;
            }

            Course = null;
            if (result.StatusCode == 404)
            {
                IsNotFound = true;
                ErrorMessage = "Curso não encontrado.";
            }
            else if (result.StatusCode == 403)
            {
                IsForbidden = true;
                ErrorMessage = "Acesso negado: permissão insuficiente para visualizar os detalhes deste curso.";
            }
            else if (result.StatusCode == 401)
            {
                IsUnauthorized = true;
                ErrorMessage = "Sessão expirada. Faça login novamente.";
            }
            else
            {
                ErrorMessage = result.Message;
            }

            return false;
        }
        catch (Exception ex)
        {
            Course = null;
            ErrorMessage = $"Erro ao carregar detalhes: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Exclui o curso atualmente exibido após confirmação na UI.
    /// </summary>
    public async Task<bool> DeleteCourseAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(CourseId) || IsDeleting)
        {
            return false;
        }

        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            IsDeleting = true;
            var result = await _courseService.DeleteAsync(CourseId, ct);

            if (result.Succeeded)
            {
                IsDeleted = true;
                SuccessMessage = "Curso excluído com sucesso!";
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao excluir curso: {ex.Message}";
            return false;
        }
        finally
        {
            IsDeleting = false;
        }
    }
}

