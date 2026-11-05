using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Courses;
using HF.Shared.Courses.Models;

namespace HF.Shared.PageModels.Courses;

/// <summary>
/// PageModel para a tela de listagem de cursos (/courses).
/// </summary>
public partial class CourseListPageModel : PageModelBase
{
    private readonly ICourseService _courseService;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsForbidden { get; set; }

    [ObservableProperty]
    public partial bool IsUnauthorized { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial CourseStatus? StatusFilter { get; set; }

    [ObservableProperty]
    public partial string? SchoolUnitIdFilter { get; set; }

    [ObservableProperty]
    public partial bool IsDeleting { get; set; }

    [ObservableProperty]
    public partial string? DeletingCourseId { get; set; }

    public ObservableCollection<CourseItemDto> Courses { get; } = new();

    public CourseListPageModel(ICourseService courseService)
    {
        _courseService = courseService;
        Title = "Cursos";
    }

    /// <summary>
    /// Retorna os cursos filtrados pelo texto de busca local (nome ou código).
    /// </summary>
    public IEnumerable<CourseItemDto> FilteredCourses
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return Courses;
            }

            var query = SearchText.Trim();
            return Courses.Where(c =>
                c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                c.Code.Contains(query, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Carrega a lista de cursos da API aplicando os filtros de Status e SchoolUnitId.
    /// </summary>
    public async Task<bool> LoadCoursesAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsForbidden = false;
        IsUnauthorized = false;

        try
        {
            IsBusy = true;
            var filter = new CourseFilterDto(
                SchoolUnitId: string.IsNullOrWhiteSpace(SchoolUnitIdFilter) ? null : SchoolUnitIdFilter.Trim(),
                Status: StatusFilter);

            var result = await _courseService.ListAsync(filter, ct);

            if (result.Succeeded && result.Data is not null)
            {
                Courses.Clear();
                foreach (var course in result.Data)
                {
                    Courses.Add(course);
                }
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
            IsBusy = false;
        }
    }

    /// <summary>
    /// Exclui um curso após a confirmação na UI.
    /// </summary>
    public async Task<bool> DeleteCourseAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id) || IsDeleting)
        {
            return false;
        }

        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            IsDeleting = true;
            DeletingCourseId = id;

            var result = await _courseService.DeleteAsync(id, ct);

            if (result.Succeeded)
            {
                var existing = Courses.FirstOrDefault(c => c.Id == id);
                if (existing is not null)
                {
                    Courses.Remove(existing);
                }

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
            DeletingCourseId = null;
        }
    }

    /// <summary>
    /// Reseta todos os filtros e recarrega os cursos.
    /// </summary>
    public async Task ResetFiltersAsync(CancellationToken ct = default)
    {
        SearchText = string.Empty;
        StatusFilter = null;
        SchoolUnitIdFilter = null;
        await LoadCoursesAsync(ct);
    }
}

