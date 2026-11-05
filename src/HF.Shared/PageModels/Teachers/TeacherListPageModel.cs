using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Teachers;
using HF.Shared.Teachers.Models;

namespace HF.Shared.PageModels.Teachers;

/// <summary>
/// PageModel para a tela de listagem de professores (/teachers).
/// </summary>
public partial class TeacherListPageModel : PageModelBase
{
    private readonly ITeacherService _teacherService;

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
    public partial TeacherStatus? StatusFilter { get; set; }

    [ObservableProperty]
    public partial string? SchoolUnitIdFilter { get; set; }

    [ObservableProperty]
    public partial string? SpecialtyFilter { get; set; }

    [ObservableProperty]
    public partial bool IsDeleting { get; set; }

    [ObservableProperty]
    public partial string? DeletingTeacherId { get; set; }

    public ObservableCollection<TeacherItemDto> Teachers { get; } = new();

    public TeacherListPageModel(ITeacherService teacherService)
    {
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        Title = "Professores";
    }

    /// <summary>
    /// Retorna os professores filtrados pelo texto de busca local (matrícula, especialidade, formação ou unidade).
    /// </summary>
    public IEnumerable<TeacherItemDto> FilteredTeachers
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return Teachers;
            }

            var query = SearchText.Trim();
            return Teachers.Where(t =>
                t.EmployeeNumber.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                t.Specialty.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (t.AcademicDegree != null && t.AcademicDegree.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                t.UserId.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (t.SchoolUnitId != null && t.SchoolUnitId.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }
    }

    /// <summary>
    /// Carrega a lista de professores da API aplicando os filtros de Status, SchoolUnitId e Specialty.
    /// </summary>
    public async Task<bool> LoadTeachersAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsForbidden = false;
        IsUnauthorized = false;

        try
        {
            IsBusy = true;
            var filter = new TeacherFilterDto(
                SchoolUnitId: string.IsNullOrWhiteSpace(SchoolUnitIdFilter) ? null : SchoolUnitIdFilter.Trim(),
                Status: StatusFilter,
                Specialty: string.IsNullOrWhiteSpace(SpecialtyFilter) ? null : SpecialtyFilter.Trim());

            var result = await _teacherService.ListAsync(filter, ct);

            if (result.Succeeded && result.Data is not null)
            {
                Teachers.Clear();
                foreach (var teacher in result.Data)
                {
                    Teachers.Add(teacher);
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
    /// Exclui um professor da API pelo identificador único.
    /// </summary>
    public async Task<bool> DeleteTeacherAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        ErrorMessage = null;
        SuccessMessage = null;
        IsForbidden = false;
        IsUnauthorized = false;

        try
        {
            IsDeleting = true;
            DeletingTeacherId = id;

            var result = await _teacherService.DeleteAsync(id, ct);
            if (result.Succeeded)
            {
                var existing = Teachers.FirstOrDefault(t => t.Id == id);
                if (existing != null)
                {
                    Teachers.Remove(existing);
                }

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
            DeletingTeacherId = null;
        }
    }

    /// <summary>
    /// Reseta todos os filtros e recarrega os dados.
    /// </summary>
    public async Task ResetFiltersAsync(CancellationToken ct = default)
    {
        SearchText = string.Empty;
        StatusFilter = null;
        SchoolUnitIdFilter = null;
        SpecialtyFilter = null;
        await LoadTeachersAsync(ct);
    }
}

