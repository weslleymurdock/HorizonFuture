using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using HF.Shared.Subjects;
using HF.Shared.Subjects.Models;

namespace HF.Shared.PageModels.Subjects;

/// <summary>
/// PageModel for the subjects list screen (/subjects).
/// </summary>
public partial class SubjectListPageModel : PageModelBase
{
    private readonly ISubjectService _subjectService;

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
    public partial SubjectStatus? StatusFilter { get; set; }

    [ObservableProperty]
    public partial string? CourseIdFilter { get; set; }

    [ObservableProperty]
    public partial string? SchoolUnitIdFilter { get; set; }

    [ObservableProperty]
    public partial bool IsDeleting { get; set; }

    [ObservableProperty]
    public partial string? DeletingSubjectId { get; set; }

    public ObservableCollection<SubjectItemDto> Subjects { get; } = new();

    public SubjectListPageModel(ISubjectService subjectService)
    {
        _subjectService = subjectService;
        Title = "Disciplinas";
    }

    /// <summary>
    /// Returns the subjects filtered by client-side search text (name or code).
    /// </summary>
    public IEnumerable<SubjectItemDto> FilteredSubjects
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return Subjects;
            }

            var query = SearchText.Trim();
            return Subjects.Where(s =>
                s.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                s.Code.Contains(query, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Loads subjects from the API applying CourseId, SchoolUnitId, and Status filters.
    /// </summary>
    public async Task<bool> LoadSubjectsAsync(CancellationToken ct = default)
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsForbidden = false;
        IsUnauthorized = false;

        try
        {
            IsBusy = true;
            var filter = new SubjectFilterDto(
                CourseId: string.IsNullOrWhiteSpace(CourseIdFilter) ? null : CourseIdFilter.Trim(),
                SchoolUnitId: string.IsNullOrWhiteSpace(SchoolUnitIdFilter) ? null : SchoolUnitIdFilter.Trim(),
                Status: StatusFilter);

            var result = await _subjectService.ListAsync(filter, ct);

            if (result.Succeeded && result.Data is not null)
            {
                Subjects.Clear();
                foreach (var subject in result.Data)
                {
                    Subjects.Add(subject);
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
    /// Deletes a subject after confirmation in the UI.
    /// </summary>
    public async Task<bool> DeleteSubjectAsync(string id, CancellationToken ct = default)
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
            DeletingSubjectId = id;

            var result = await _subjectService.DeleteAsync(id, ct);

            if (result.Succeeded)
            {
                var existing = Subjects.FirstOrDefault(s => s.Id == id);
                if (existing is not null)
                {
                    Subjects.Remove(existing);
                }

                SuccessMessage = "Disciplina excluída com sucesso!";
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao excluir disciplina: {ex.Message}";
            return false;
        }
        finally
        {
            IsDeleting = false;
            DeletingSubjectId = null;
        }
    }

    /// <summary>
    /// Resets all filters and reloads the subjects list.
    /// </summary>
    public async Task ResetFiltersAsync(CancellationToken ct = default)
    {
        SearchText = string.Empty;
        StatusFilter = null;
        CourseIdFilter = null;
        SchoolUnitIdFilter = null;
        await LoadSubjectsAsync(ct);
    }
}

