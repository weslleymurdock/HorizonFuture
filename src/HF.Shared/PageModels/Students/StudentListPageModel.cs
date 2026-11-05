using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Students;
using HF.Shared.Students.Models;

namespace HF.Shared.PageModels.Students;

/// <summary>
/// PageModel for the students list screen (/students).
/// </summary>
public partial class StudentListPageModel : PageModelBase
{
    private readonly IStudentService _studentService;

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
    public partial StudentStatus? StatusFilter { get; set; }

    [ObservableProperty]
    public partial string? SchoolUnitIdFilter { get; set; }

    [ObservableProperty]
    public partial bool IsDeleting { get; set; }

    [ObservableProperty]
    public partial string? DeletingStudentId { get; set; }

    public ObservableCollection<StudentItemDto> Students { get; } = new();

    public StudentListPageModel(IStudentService studentService)
    {
        _studentService = studentService;
        Title = "Alunos";
    }

    /// <summary>
    /// Returns the students filtered by client-side search text (registration number, user ID, guardian).
    /// </summary>
    public IEnumerable<StudentItemDto> FilteredStudents
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return Students;
            }

            var query = SearchText.Trim();
            return Students.Where(s =>
                s.RegistrationNumber.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                s.UserId.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (s.GuardianName != null && s.GuardianName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (s.SchoolUnitId != null && s.SchoolUnitId.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }
    }

    /// <summary>
    /// Loads students from the API applying SchoolUnitId and Status filters.
    /// </summary>
    public async Task<bool> LoadStudentsAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsForbidden = false;
        IsUnauthorized = false;

        try
        {
            IsBusy = true;
            var filter = new StudentFilterDto(
                SchoolUnitId: string.IsNullOrWhiteSpace(SchoolUnitIdFilter) ? null : SchoolUnitIdFilter.Trim(),
                Status: StatusFilter);

            var result = await _studentService.ListAsync(filter, ct);

            if (result.Succeeded && result.Data is not null)
            {
                Students.Clear();
                foreach (var student in result.Data)
                {
                    Students.Add(student);
                }
                return true;
            }

            Students.Clear();
            if (result.StatusCode == 403)
            {
                IsForbidden = true;
                ErrorMessage = "Acesso negado: permissão Students.Read ausente.";
            }
            else if (result.StatusCode == 401)
            {
                IsUnauthorized = true;
                ErrorMessage = "Sessão expirada. Faça login novamente para continuar.";
            }
            else
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : "Falha ao carregar listagem de alunos.";
            }

            return false;
        }
        catch (Exception ex)
        {
            Students.Clear();
            ErrorMessage = $"Erro inesperado: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Deletes a student by unique identifier.
    /// </summary>
    public async Task<bool> DeleteStudentAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            ErrorMessage = "Identificador do aluno é inválido.";
            return false;
        }

        if (IsDeleting)
        {
            return false;
        }

        ErrorMessage = null;
        SuccessMessage = null;
        DeletingStudentId = id;
        IsDeleting = true;

        try
        {
            var result = await _studentService.DeleteAsync(id, ct);

            if (result.Succeeded)
            {
                var existing = Students.FirstOrDefault(s => s.Id == id);
                if (existing is not null)
                {
                    Students.Remove(existing);
                }
                SuccessMessage = "Aluno excluído com sucesso.";
                return true;
            }

            if (result.StatusCode == 409)
            {
                ErrorMessage = "Não é possível excluir o aluno pois existem matrículas vinculadas ou histórico acadêmico ativo.";
            }
            else if (result.StatusCode == 404)
            {
                ErrorMessage = "Aluno não encontrado ou já foi excluído.";
            }
            else if (result.StatusCode == 403)
            {
                ErrorMessage = "Acesso negado: você não possui permissão para excluir alunos (Students.Delete).";
            }
            else
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(result.Message)
                    ? result.Message
                    : "Falha ao excluir aluno.";
            }

            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao tentar excluir aluno: {ex.Message}";
            return false;
        }
        finally
        {
            IsDeleting = false;
            DeletingStudentId = null;
        }
    }

    /// <summary>
    /// Resets all filters and reloads the student list.
    /// </summary>
    public async Task ResetFiltersAsync(CancellationToken ct = default)
    {
        SearchText = string.Empty;
        StatusFilter = null;
        SchoolUnitIdFilter = null;
        await LoadStudentsAsync(ct);
    }
}

